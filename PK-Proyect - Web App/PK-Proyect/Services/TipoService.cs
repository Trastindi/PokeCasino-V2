using MongoDB.Driver;
using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    public static class TipoService
    {
        private static readonly IMongoCollection<Tipo> _tipos;

        static TipoService()
        {
            var client = new MongoClient("mongodb+srv://marcosemiliorodriguezmartin_db_user:gDfjWHYHIqMJ346V@pokecasino.asaeily.mongodb.net");
            var db = client.GetDatabase("PokemonDB");
            _tipos = db.GetCollection<Tipo>("Tipos");
        }

        public static List<Tipo> GetAll()
        {
            return _tipos.Find(_ => true).ToList();
        }

        public static Tipo GetByNombre(string nombre)
        {
            return _tipos.Find(t => t.Nombre == nombre).FirstOrDefault();
        }

        public static void InsertMany(List<Tipo> lista)
        {
            _tipos.InsertMany(lista);
        }
    }
}
