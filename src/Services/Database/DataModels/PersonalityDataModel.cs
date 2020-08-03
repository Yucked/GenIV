using GenIVIV.Services.Database.Interfaces;

namespace GenIVIV.Services.Database.DataModels {
    public struct PersonalityDataModel : IDataModel {
        public string Id { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
    }
}