using GenIVIV.Services.Database.Interfaces;

namespace GenIVIV.Services.Database.DataModels {
    public sealed class ConfigDataModel : IDataModel {
        public string Id { get; set; }
        public string Token { get; set; }
        public char Prefix { get; set; }
        public bool IsMentionEnabled { get; set; }
    }
}