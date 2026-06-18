using PK_Proyect.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la colección PokemonUser a través del servidor Flask.
    /// </summary>
    public class PokemonUserRepository
    {
        /// <summary>
        /// Devuelve los pokémon del usuario autenticado (usa JWT, no necesita userId).
        /// Endpoint: GET /usuarios/mis_pokemon
        /// </summary>
        public List<PokemonUser> GetMisPokemon()
            => ApiClient.Get<List<PokemonUser>>("/usuarios/mis_pokemon");

        /// <summary>
        /// Devuelve los pokémon de cualquier usuario por su ID (requiere ser admin o el propio usuario).
        /// Endpoint: GET /usuarios/{userId}/pokemon
        /// </summary>
        public List<PokemonUser> GetPokemonsByUser(string userId)
            => ApiClient.Get<List<PokemonUser>>($"/usuarios/{userId}/pokemon");

        public PokemonUser GetPokemon(string userId, int pokemonId)
        {
            var lista = GetPokemonsByUser(userId);
            return lista.Find(p => p.PokemonId == pokemonId);
        }

        public PokemonUser InsertPokemon(PokemonUser pokemon)
            => ApiClient.Post<PokemonUser>("/pokemon/obtener", new
            {
                pokemon_id = pokemon.PokemonId,
                nombre     = pokemon.Nombre,
                tipo1      = pokemon.TipoPrincipal,
                tipo2      = pokemon.TipoSecundario
            });

        /// <summary>
        /// Actualización de moveset.
        /// </summary>
        public void UpdatePokemon(PokemonUser pokemon, int pokemonIdOriginal)
        {
            ApiClient.Put<object>("/pokemon/movimiento", new
            {
                pokemon_id       = pokemonIdOriginal,
                indice_a_borrar  = -1,
                movimiento_nuevo = string.Join(",", pokemon.MoveSet)
            });
        }

        public void UpdatePokemon(PokemonUser pokemon)
            => UpdatePokemon(pokemon, pokemon.PokemonId);

        public int CountByType(string userId, string tipo)
        {
            var lista = GetPokemonsByUser(userId);
            return lista.FindAll(p => p.TipoPrincipal == tipo).Count;
        }
    }
}
