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
            _zonas = MongoDbContext.GetCollection<Zona>("Zonas");
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
