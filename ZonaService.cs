using MongoDB.Driver;
using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    public static class ZonaService
    {
        private static readonly IMongoCollection<Zona> _zonas;

        static ZonaService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("PokemonDB");
            _zonas = db.GetCollection<Zona>("Zonas");
        }

        public static List<Zona> GetAll()
        {
            return _zonas.Find(_ => true).ToList();
        }

        public static Zona GetById(string id)
        {
            return _zonas.Find(z => z.Id == id).FirstOrDefault();
        }

        public static void InsertMany(List<Zona> lista)
        {
            _zonas.InsertMany(lista);
        }
    }
}
