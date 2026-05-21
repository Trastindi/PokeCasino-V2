using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class PokedexRepository
    {
        private readonly IMongoCollection<Pokemon> _pokedex;

        public PokedexRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("PokemonDB");
            _pokedex = database.GetCollection<Pokemon>("Pokedex");
        }

        public Pokemon ObtenerPorId(int Id)
        {
            return _pokedex.Find(p => p.Id == Id).FirstOrDefault();
        }

        public List<Pokemon> ObtenerTodos() { return _pokedex.Find(p => true).ToList(); }
    }
}
