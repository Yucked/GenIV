using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using GenIVIV.Attributes;
using GenIVIV.Logging;
using GenIVIV.Services;
using GenIVIV.Services.Database;
using GenIVIV.Services.Database.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Victoria;
using Console = Colorful.Console;

namespace GenIVIV {
    internal sealed class Program {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public Program() {
            Extensions.PrintInformationalHeader();

            var serviceCollection = new ServiceCollection()
                .AddLavaNode(x => {
                    x.ReconnectAttempts = 3;
                    x.ReconnectDelay = TimeSpan.FromSeconds(3);
                })
                .AddLogging(x => {
                    x.ClearProviders();
                    x.AddProvider(new LoggerProvider());
                })
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddServices<InjectableServiceAttribute>();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<Program>();
        }

        private static Task Main() {
            return new Program().RunAsync();
        }

        private async Task RunAsync() {
            await CreateConfigurationAsync();
            await InitializeDiscordAsync();

            await Task.Delay(-1);
        }

        private async Task CreateConfigurationAsync() {
            var databaseService = _serviceProvider.GetRequiredService<DatabaseService>();
            if (DatabaseService.Exists<ConfigDataModel>("Configuration")) {
                return;
            }

            var config = new ConfigDataModel {
                Id = "Configuration"
            };

            if (!Extensions.TryTakeInput("Please enter your bot token: ", out var token)) {
                _logger.LogCritical("A bot token must be provided!");
                await ExitAsync();
            }

            config.Token = token;
            config.Prefix = Extensions.TryTakeInput("Please enter default prefix: ", out var prefix)
                ? prefix[0]
                : '?';

            if (Extensions.TryTakeInput("Would you like to enable mention prefix? (Y/N): ", out var enableMention)) {
                var input = char.ToLower(enableMention[0]);
                var isEnabled = input == 'y';
                config.IsMentionEnabled = isEnabled;
            }
            
            if (databaseService.TryStore(config)) {
                Console.Clear();
                Extensions.PrintInformationalHeader();
                return;
            }

            await ExitAsync();
        }

        private async Task InitializeDiscordAsync() {
            var discordService = _serviceProvider.GetRequiredService<DiscordService>();
            await discordService.InitializeAsync();
        }

        private async Task ExitAsync() {
            _logger.LogCritical("No configuration found! Exiting in 5 seconds ...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Environment.Exit(-1);
        }
    }
}