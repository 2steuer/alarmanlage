// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using SteuerSoft.AlarmSystem;
using SteuerSoft.AlarmSystem.TelegramBot;

var cfg = new ConfigurationBuilder()
    .AddJsonFile("config.json")
    .AddJsonFile("config.overrides.json", optional: true)
    .Build();

var telegram = new AlarmSystemBot(cfg["Telegram:ApiKey"], cfg["Telegram:Password"]);
telegram.Start();

Console.ReadLine();