using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class PokedexRepository
    {
        private readonly IMongoCollection<Pokemon> _pokedex;

        public PokedexRepository()
        {
            _pokedex = MongoDbContext.GetCollection<Pokemon>("Pokedex");
        }

        public Pokemon ObtenerPorId(int Id)
        {
            return _pokedex.Find(p => p.numero_pokedex == Id).FirstOrDefault();
        }

        public List<Pokemon> ObtenerTodos()
        {
            return _pokedex.Find(p => true).ToList();
        }
    }
}
