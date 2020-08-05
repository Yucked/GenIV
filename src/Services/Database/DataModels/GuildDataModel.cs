using GenIVIV.Services.Database.Interfaces;

namespace GenIVIV.Services.Database.DataModels {
    public struct GuildDataModel : IDataModel {
        public string Id { get; set; }
        public char Prefix { get; set; }
        public string WelcomeMessage { get; set; }
    }
}