using MongoDB.Driver;
using System;

namespace PK_Proyect
{
    /// <summary>
    /// Punto único de conexión a MongoDB Atlas.
    /// La connection string se lee de la variable de entorno MONGO_URI
    /// (definída en App.config, en el entorno del sistema o en un archivo .env
    /// cargado al iniciar la aplicación).
    /// </summary>
    public static class MongoDbContext
    {
        private static readonly string ConnectionString =
            Environment.GetEnvironmentVariable("MONGO_URI")
            ?? throw new InvalidOperationException(
                "La variable de entorno MONGO_URI no está configurada. "
              + "Añádela al entorno del sistema o a App.config antes de ejecutar la aplicación.");

        private const string DatabaseName = "PokemonDB";

        private static readonly Lazy<IMongoDatabase> _db = new(() =>
            new MongoClient(ConnectionString).GetDatabase(DatabaseName));

        public static IMongoDatabase Database => _db.Value;

        public static IMongoCollection<T> GetCollection<T>(string name) =>
            Database.GetCollection<T>(name);
    }
}
