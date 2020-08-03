using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GenIVIV.Attributes;
using GenIVIV.Services.Database;
using GenIVIV.Services.Database.DataModels;
using Microsoft.Extensions.Logging;
using Victoria;

namespace GenIVIV.Services {
    [InjectableService]
    public sealed class DiscordService {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _socketClient;
        private readonly CommandService _commandService;
        private readonly DatabaseService _databaseService;
        private readonly ILogger _logger;
        private readonly ConfigDataModel _configData;

        public DiscordService(IServiceProvider serviceProvider, DiscordSocketClient socketClient,
                              CommandService commandService, ILoggerFactory loggerFactory,
                              DatabaseService databaseService, ILogger<DiscordService> logger) {
            _serviceProvider = serviceProvider;
            _socketClient = socketClient;
            _commandService = commandService;
            _databaseService = databaseService;
            _logger = logger;

            var commandLogger = loggerFactory.CreateLogger<CommandService>();
            var discordLogger = loggerFactory.CreateLogger<DiscordSocketClient>();

            _commandService.Log += logMessage => {
                commandLogger.Log(logMessage.Severity.FromSeverityToLevel(), logMessage.Exception, logMessage.Message);
                return Task.CompletedTask;
            };

            _socketClient.Log += logMessage => {
                discordLogger.Log(logMessage.Severity.FromSeverityToLevel(), logMessage.Exception, logMessage.Message);
                return Task.CompletedTask;
            };

            _socketClient.Ready += OnReadyAsync;
            _socketClient.MessageReceived += OnMessageReceivedAsync;

            _databaseService.TryGet("Configuration", out _configData);
        }

        public async Task InitializeAsync() {
            await _commandService.AddModulesAsync(typeof(Program).Assembly, _serviceProvider);

            await _socketClient.LoginAsync(TokenType.Bot, _configData.Token);
            await _socketClient.StartAsync();
        }

        private async Task OnReadyAsync() {
            await _serviceProvider.UseLavaNodeAsync();
        }

        private async Task OnMessageReceivedAsync(SocketMessage socketMessage) {
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook ||
                !(socketMessage is SocketUserMessage userMessage)) {
                return;
            }

            var argPos = 0;
            var guild = (userMessage.Channel as IGuildChannel).Guild;

            if (!userMessage.HasCharPrefix(_configData.Prefix, ref argPos)
                && !(_configData.IsMentionEnabled &&
                     userMessage.HasMentionPrefix(_socketClient.CurrentUser, ref argPos))
                && !(_databaseService.TryGet<GuildDataModel>($"{guild.Id}", out var guildData) &&
                    userMessage.HasCharPrefix(guildData.Prefix, ref argPos))) {
                return;
            }

            var commandContext = new SocketCommandContext(_socketClient, userMessage);
            await _commandService.ExecuteAsync(commandContext, argPos, _serviceProvider, MultiMatchHandling.Best);
        }
    }
}