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
        public PokemonUser GetPokemon(string userId, int pokemonId)
        {
            var lista = GetPokemonsByUser(userId);
            return lista.Find(p => p.PokemonId == pokemonId);
        }

        public List<PokemonUser> GetPokemonsByUser(string userId)
            => ApiClient.Get<List<PokemonUser>>($"/usuarios/{userId}/pokemon");

        public void InsertPokemon(PokemonUser pokemon)
            => ApiClient.Post<object>("/pokemon/obtener", new
            {
                pokemon_id = pokemon.PokemonId,
                nombre     = pokemon.Nombre,
                tipo1      = pokemon.TipoPrincipal,
                tipo2      = pokemon.TipoSecundario,
                current_hp = pokemon.CurrentHp
            });

        /// <summary>
        /// Actualización de moveset. Para subida de nivel usa InsertPokemon (que llama a /pokemon/obtener).
        /// </summary>
        public void UpdatePokemon(PokemonUser pokemon, int pokemonIdOriginal)
        {
            // Solo se actualiza el moveset directamente
            // Las subidas de nivel se delegan a /pokemon/obtener
            ApiClient.Put<object>("/pokemon/movimiento", new
            {
                pokemon_id     = pokemonIdOriginal,
                indice_a_borrar = -1,          // sin reemplazo, solo sincroniza
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
