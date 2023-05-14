using NLog;
using SteuerSoft.AlarmSystem.Core.Interfaces;

namespace SteuerSoft.AlarmSystem.Core.Sequences;

public class Sequence
{
    private ILogger _log;

    public bool Repeat { get; }

    public string Name { get; }

    public bool Running { get; private set; }

    private List<ISequenceEntry> _entries = new List<ISequenceEntry>();

    private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

    private TaskCompletionSource<bool> _runnerCompletionSource = new TaskCompletionSource<bool>();

    public Sequence(string name, bool repeat)
    {
        Name = name;
        Repeat = repeat;

        _log = LogManager.GetLogger($"Sequence:{name}");
    }

    public Sequence Add(ISequenceEntry entry)
    {
        _entries.Add(entry);

        return this;
    }

    public Task Start()
    {
        lock (this)
        {
            if (Running)
            {
                return Task.CompletedTask;
            }

            Running = true;
            _runnerCompletionSource = new TaskCompletionSource<bool>();
            _cancelTokenSource = new CancellationTokenSource();
        }

        ThreadPool.QueueUserWorkItem(RunnerThread);
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        lock (this)
        {
            if (!Running)
            {
                return;
            }
        }

        if (_cancelTokenSource.IsCancellationRequested)
        {
            return;
        }

        _cancelTokenSource.Cancel();

        await _runnerCompletionSource.Task;

        try
        {
            await Task.WhenAll(_entries.Select(e => e.Reset(CancellationToken.None)));
        }
        catch (Exception e)
        {
            _log.Error(e, $"Error while resetting sequence.");
        }
    }

    private async void RunnerThread(object? state)
    {
        try
        {
            _log.Debug($"Starting sequence.");

            // If Repeat == false, executes everything just once
            do
            {
                foreach (ISequenceEntry entry in _entries)
                {
                    await entry.Execute(_cancelTokenSource.Token);
                }
            } while (Repeat && !_cancelTokenSource.Token.IsCancellationRequested);
        }
        catch (TaskCanceledException)
        {
            _log.Debug($"Stopped sequence.");
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Error in sequence.");

        }
        finally
        {
            lock (this)
            {
                _runnerCompletionSource.SetResult(false);
                Running = false;
                _runnerCompletionSource = null;
            }
        }
    }
}