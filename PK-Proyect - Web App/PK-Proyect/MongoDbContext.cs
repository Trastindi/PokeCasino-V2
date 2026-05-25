using MongoDB.Driver;

namespace PK_Proyect
{
    /// <summary>
    /// Punto único de conexión a MongoDB Atlas.
    /// Cambia ConnectionString aquí para afectar a todos los repositorios.
    /// </summary>
    public static class MongoDbContext
    {
        private const string ConnectionString =
            "mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net";

        private const string DatabaseName = "PokemonDB";

        private static readonly Lazy<IMongoDatabase> _db = new(() =>
            new MongoClient(ConnectionString).GetDatabase(DatabaseName));

        public static IMongoDatabase Database => _db.Value;

        public static IMongoCollection<T> GetCollection<T>(string name) =>
            Database.GetCollection<T>(name);
    }
}
