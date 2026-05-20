using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class PokemonUserRepository
    {
        private readonly IMongoCollection<PokemonUser> _collection;

        public PokemonUserRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("PokemonDB");
            _collection = db.GetCollection<PokemonUser>("PokemonUser");
        }

        public PokemonUser GetPokemon(string userId, int pokemonId)
        {
            return _collection.Find(p => p.UserId == userId && p.PokemonId == pokemonId).FirstOrDefault();
        }

        public List<PokemonUser> GetPokemonsByUser(string userId)
        {
            return _collection.Find(p => p.UserId == userId).ToList();
        }

        public void InsertPokemon(PokemonUser pokemon)
        {
            _collection.InsertOne(pokemon);
        }

        public void UpdatePokemon(PokemonUser pokemon)
        {
            _collection.ReplaceOne(p => p.Id == pokemon.Id, pokemon);
        }

        public int CountByType(string userId, string tipo)
        {
            return (int)_collection.CountDocuments(p => p.UserId == userId && p.TipoPrincipal == tipo);

        }
    }
}
