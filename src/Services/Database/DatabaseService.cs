using GenIVIV.Attributes;
using GenIVIV.Services.Database.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace GenIVIV.Services.Database {
    [InjectableService]
    public sealed class DatabaseService {
        private const string DATABASE_NAME = "GenIVIV.db";
        private readonly ILogger _logger;

        public DatabaseService(ILogger<DatabaseService> logger) {
            _logger = logger;
        }

        public bool TryGet<T>(object id, out T data) where T : IDataModel {
            data = default;

            using var database = new LiteDatabase(DATABASE_NAME);
            var collection = database.GetCollection<T>(Extensions.GetNameOfT<T>());
            if (!collection.Exists(x => x.Id == $"{x.Id}")) {
                _logger.LogError($"Couldn't find document with {id} id.");
                return false;
            }

            var document = collection.FindById($"{id}");
            if (document == null) {
                _logger.LogError($"Document of type {Extensions.GetNameOfT<T>()} returned null.");
                return false;
            }

            data = document;
            return true;
        }

        public bool TryStore<T>(T data) where T : IDataModel {
            if (data == null) {
                _logger.LogError($"Data of type {Extensions.GetNameOfT<T>()} was null.");
                return false;
            }

            using var database = new LiteDatabase(DATABASE_NAME);
            var collection = database.GetCollection<T>(Extensions.GetNameOfT<T>());
            if (collection.Exists(x => x.Id == data.Id)) {
                _logger.LogError($"Document with {data.Id} already exists.");
                return false;
            }

            collection.Insert(data);
            return true;
        }

        public static bool TryUpdate<T>(T data) where T : IDataModel {
            using var database = new LiteDatabase(DATABASE_NAME);
            var collection = database.GetCollection<T>(Extensions.GetNameOfT<T>());
            return collection.Update(data);
        }

        public static bool TryDelete<T>(object id) where T : IDataModel {
            using var database = new LiteDatabase(DATABASE_NAME);
            var collection = database.GetCollection<T>(Extensions.GetNameOfT<T>());
            return collection.Delete($"{id}");
        }

        public static bool Exists<T>(object id) where T : IDataModel {
            using var database = new LiteDatabase(DATABASE_NAME);
            var collection = database.GetCollection<T>(Extensions.GetNameOfT<T>());
            return collection.Exists(x => x.Id == $"{id}");
        }
    }
}