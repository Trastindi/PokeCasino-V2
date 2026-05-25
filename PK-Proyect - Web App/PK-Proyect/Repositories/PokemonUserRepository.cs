using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public class PokemonUserRepository
    {
        private readonly IMongoCollection<PokemonUser> _collection;

        public PokemonUserRepository()
        {
            _collection = MongoDbContext.GetCollection<PokemonUser>("PokemonUser");
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

        /// <summary>
        /// Reemplaza el documento identificado por (userId, pokemonIdOriginal).
        /// Usar pokemonIdOriginal cuando el pokemon puede haber evolucionado
        /// y su PokemonId ya fue cambiado al de la evolucion.
        /// </summary>
        public void UpdatePokemon(PokemonUser pokemon, int pokemonIdOriginal)
        {
            _collection.ReplaceOne(
                p => p.UserId == pokemon.UserId && p.PokemonId == pokemonIdOriginal,
                pokemon);
        }

        // Sobrecarga sin cambio de id (casos donde no hay evolucion)
        public void UpdatePokemon(PokemonUser pokemon)
            => UpdatePokemon(pokemon, pokemon.PokemonId);

        public int CountByType(string userId, string tipo)
        {
            return (int)_collection.CountDocuments(p => p.UserId == userId && p.TipoPrincipal == tipo);
        }
    }
}
