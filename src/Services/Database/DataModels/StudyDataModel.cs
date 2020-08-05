using System.Collections.Generic;
using GenIVIV.Services.Database.Interfaces;

namespace GenIVIV.Services.Database.DataModels {
    public struct StudyDataModel : IDataModel{
        public string Id { get; set; }
        public IDictionary<ulong, StudyMessageDataModel> UserMessages { get; set; }
    }

    public struct StudyMessageDataModel {
        public string Username { get; set; }
        public HashSet<string> Messages { get; set; }
    }
}