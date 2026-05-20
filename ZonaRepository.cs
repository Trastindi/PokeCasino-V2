using MongoDB.Driver;
using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public class ZonaRepository : IZonaRepository
    {
        private readonly IMongoCollection<Zona> _zonas;

        public ZonaRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("PokemonDB");
            _zonas = database.GetCollection<Zona>("Zonas");
        }

        public List<Zona> ObtenerTodas()
        {
            return _zonas.Find(z => true).ToList();
        }

        public Zona ObtenerPorNombre(string nombre)
        {
            return _zonas.Find(z => z.Nombre == nombre).FirstOrDefault();
        }
    }
}
