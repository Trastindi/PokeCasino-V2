using MongoDB.Driver;
using System;
using System.IO;

namespace PK_Proyect
{
    /// <summary>
    /// Punto \u00fanico de conexi\u00f3n a MongoDB Atlas.
    /// La connection string se resuelve en este orden:
    ///   1. Variable de entorno MONGO_URI del sistema.
    ///   2. Archivo "mongo.env" en el mismo directorio que el ejecutable
    ///      (formato clave=valor, una por l\u00ednea, p.ej: MONGO_URI=mongodb+srv://...).
    /// </summary>
    public static class MongoDbContext
    {
        private static readonly string ConnectionString = ResolverConnectionString();

        private const string DatabaseName = "PokemonDB";

        private static readonly Lazy<IMongoDatabase> _db = new(() =>
            new MongoClient(ConnectionString).GetDatabase(DatabaseName));

        public static IMongoDatabase Database => _db.Value;

        public static IMongoCollection<T> GetCollection<T>(string name) =>
            Database.GetCollection<T>(name);

        private static string ResolverConnectionString()
        {
            // 1. Variable de entorno del sistema
            var fromEnv = Environment.GetEnvironmentVariable("MONGO_URI");
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv;

            // 2. Archivo mongo.env junto al ejecutable
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var envFile = Path.Combine(exeDir, "mongo.env");
            if (File.Exists(envFile))
            {
                foreach (var linea in File.ReadAllLines(envFile))
                {
                    if (string.IsNullOrWhiteSpace(linea) || linea.TrimStart().StartsWith("#"))
                        continue;

                    var idx = linea.IndexOf('=');
                    if (idx < 1) continue;

                    var clave = linea.Substring(0, idx).Trim();
                    var valor = linea.Substring(idx + 1).Trim();

                    if (clave.Equals("MONGO_URI", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(valor))
                        return valor;
                }
            }

            // 3. Ninguno disponible
            throw new InvalidOperationException(
                "No se encontr\u00f3 la cadena de conexi\u00f3n a MongoDB.\n\n" +
                "Opciones:\n" +
                "  A) Define la variable de entorno MONGO_URI en el sistema.\n" +
                $"  B) Crea el archivo \"mongo.env\" en:\n     {exeDir}\n" +
                "     con el contenido:\n" +
                "     MONGO_URI=mongodb+srv://usuario:password@cluster...");
        }
    }
}
