using System;
using MaxLib.Ini.Parser;
using MaxLib.Ini;
using System.IO;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Mabron.DiscordBots
{
    class Program
    {
        public static DiscordSocketClient? DiscordClient { get; private set; }

        public static IniFile? Config { get; private set; }

        static async Task Main(string[] args)
        {
            if (!File.Exists("config.ini"))
            {
                Console.Error.WriteLine("config.ini not found");
                return;
            }
            var config = Config = new IniParser().Parse("config.ini");
            var token = config[0].GetString("token", null);
            if (token == null)
            {
                Console.Error.WriteLine("no token in config set");
                return;
            }

            LogProvider.SetCurrentLogProvider(new Logger());

            using var client = DiscordClient = new DiscordSocketClient();
            client.Log += Client_Log;
            var commands = new CommandService();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            var service = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();

            await service.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();
            await InitSchedules(scheduler);

            Games.Werwolf.GameServer.Start();
            MaxLib.WebServer.WebServerLog.LogPreAdded += WebServerLog_LogPreAdded;

            using var canceller = new CancellationTokenSource();
            _ = Task.Run(() =>
            {
                while (Console.ReadKey().Key != ConsoleKey.Q) ;
                Console.Write('\b');
                canceller.Cancel();
            });
            Console.CancelKeyPress += (_, e) => canceller.Cancel();
            try { await Task.Delay(-1, canceller.Token); }
            catch (TaskCanceledException) { }

            await scheduler.Shutdown();
            Shop.ShopCommand.Dispose();
            Games.Werwolf.GameServer.Stop();
        }

        private static async Task InitSchedules(IScheduler scheduler)
        {
            var job = JobBuilder.Create<Shop.Jobs.SyncGlobalList>()
                .WithIdentity("job1", "group1")
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)
                    .WithMisfireHandlingInstructionFireNow()
                    .RepeatForever()
                )
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        private static async Task Client_Log(LogMessage msg)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now:G}] [{msg.Severity}] [{msg.Source}] {msg.Message} {msg.Exception}");
        }

        private static void WebServerLog_LogPreAdded(MaxLib.WebServer.ServerLogArgs eventArgs)
        {
            eventArgs.Discard = true;
            if (eventArgs.LogItem.Type == MaxLib.WebServer.ServerLogType.Debug || eventArgs.LogItem.Type == MaxLib.WebServer.ServerLogType.Information)
                return;
            Console.Out.WriteLine($"[{DateTime.Now:G}] [{eventArgs.LogItem.Type}] " +
                $"[{eventArgs.LogItem.SenderType}] {eventArgs.LogItem.InfoType}: {eventArgs.LogItem.Information}");
        }
    }

    class Logger : ILogProvider
    {
        public Quartz.Logging.Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= LogLevel.Info && func != null)
                {
                    Console.WriteLine($"[{DateTime.Now:G}] [{level}] [Quartz] {func()} {exception}", parameters);
                }
                return true;
            };
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }
    }
}
