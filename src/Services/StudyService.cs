using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using GenIVIV.Services.Database;
using GenIVIV.Services.Database.DataModels;
using Microsoft.Extensions.Logging;

namespace GenIVIV.Services {
    public sealed class StudyService {
        private readonly DiscordSocketClient _socketClient;
        private readonly DatabaseService _databaseService;
        private readonly ILogger _logger;

        public StudyService(DiscordSocketClient socketClient, DatabaseService databaseService,
                            ILogger<StudyService> logger) {
            _socketClient = socketClient;
            _databaseService = databaseService;
            _logger = logger;
            _socketClient.MessageReceived += OnMessageAsync;
        }

        private async Task OnMessageAsync(SocketMessage socketMessage) {
            if (socketMessage.Author.IsBot || socketMessage.Author.IsWebhook) {
                return;
            }

            if (!_databaseService.TryGet<StudyDataModel>("Study", out var studyDataModel)) {
                studyDataModel = new StudyDataModel {
                    Id = "Study",
                    UserMessages = new Dictionary<ulong, StudyMessageDataModel>()
                };

                var studyMessage = new StudyMessageDataModel {
                    Messages = new HashSet<string>(),
                    Username = socketMessage.Author.Username
                };

                studyMessage.Messages.Add(socketMessage.Content);
                studyDataModel.UserMessages.Add(socketMessage.Author.Id, studyMessage);
                _databaseService.TryStore(studyDataModel);
                return;
            }

            if (studyDataModel.UserMessages.TryGetValue(socketMessage.Author.Id, out var userMessages)) {
                userMessages.Messages.Add(socketMessage.Content);
                if (DatabaseService.TryUpdate(studyDataModel)) {
                    return;
                }

                _logger.LogCritical($"Failed to save document with {studyDataModel.Id}");
                return;
            }

            userMessages = new StudyMessageDataModel {
                Messages = new HashSet<string>(),
                Username = socketMessage.Author.Username
            };
            userMessages.Messages.Add(socketMessage.Content);
            studyDataModel.UserMessages.TryAdd(socketMessage.Author.Id, userMessages);
            if (DatabaseService.TryUpdate(studyDataModel)) {
                return;
            }

            _logger.LogCritical($"Failed to save document with {studyDataModel.Id}");
        }
    }
}