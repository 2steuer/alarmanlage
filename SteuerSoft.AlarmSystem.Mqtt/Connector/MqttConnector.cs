using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server.Internal;
using NLog;

namespace SteuerSoft.AlarmSystem.Mqtt.Connector;

public class MqttConnector : IPublisher
{
    private static ILogger _log = LogManager.GetCurrentClassLogger();
    private IManagedMqttClient? _client = null;

    private string _server;
    private int _port;
    private string _clientId;

    private Dictionary<string, List<ISubscriber>> _subscribers = new();

    public MqttConnector(string server, int port, string clientId)
    {
        _server = server;
        _port = port;
        _clientId = clientId;
    }

    public async Task Start()
    {
        if (_client?.IsStarted ?? false)
        {
            return;
        }

        var opt = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(copt =>
            {
                copt.WithTcpServer(_server, _port)
                    .WithClientId(_clientId);
            }).Build();
        
        _client = new MqttFactory().CreateManagedMqttClient();
        
        

        foreach (var filter in _subscribers.Keys)
        {
            await _client.SubscribeAsync(filter);
        }

        _client.UseApplicationMessageReceivedHandler(HandleMessage);

        await _client.StartAsync(opt);
    }

    private Task _client_ConnectionStateChangedAsync(EventArgs arg)
    {
        _log.Info($"MQTT Connection: {_client?.IsConnected}");

        return Task.CompletedTask;
    }

    private async Task HandleMessage(MqttApplicationMessageReceivedEventArgs arg)
    {
        var topic = arg.ApplicationMessage.Topic;

        var rcvrs = _subscribers.Where(kvp =>
                MqttTopicFilterComparer.IsMatch(topic, kvp.Key))
            .SelectMany(kvp => kvp.Value);

        var payload = arg.ApplicationMessage.ConvertPayloadToString();

        await Task.WhenAll(rcvrs.Select(r => r.HandleMessage(topic, payload)));
        arg.IsHandled = true;
    }

    public async Task AddSubscriber(ISubscriber sub)
    {
        bool haveFilter = _subscribers.ContainsKey(sub.Filter);

        if (haveFilter)
        {
            _subscribers[sub.Filter].Add(sub);
        }
        else
        {
            _subscribers.Add(sub.Filter, new List<ISubscriber>() {sub});

            // does not subscribe if client is null
            await (_client != null ? _client.SubscribeAsync(sub.Filter) : Task.CompletedTask);
        }
    }

    public async Task RemoveSubscriber(ISubscriber sub)
    {
        if (!_subscribers.ContainsKey(sub.Filter))
        {
            return;
        }

        _subscribers[sub.Filter].Remove(sub);

        if (_client != null)
        {
            await _client.UnsubscribeAsync(sub.Filter);
        }
    }

    public async Task Stop()
    {
        if (!_client?.IsStarted ?? true)
        {
            return;
        }

        await _client!.StopAsync();
        _client = null;
    }

    public Task PublishMessage(string topic, string payload, bool retain = false, CancellationToken ctx = default)
    {
        if (_client == null)
        {
            return Task.CompletedTask;
        }

        var msgb = new ManagedMqttApplicationMessageBuilder()
            .WithApplicationMessage(opt =>
            {
                opt.WithTopic(topic)
                    .WithPayload(payload)
                    .WithRetainFlag(retain)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithContentType("text/plain");
            }).Build();
        
        return _client.PublishAsync(msgb);
    }
}