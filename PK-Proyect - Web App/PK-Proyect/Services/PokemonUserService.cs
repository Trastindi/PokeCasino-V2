using PK_Proyect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    public class PokemonUserService
    {
        /// <summary>
        /// Llama al servidor para crear un nuevo PokemonUser a nivel 1.
        /// El servidor siempre inserta un documento nuevo (nunca sube de nivel).
        /// Devuelve el PokemonUser creado, o null si falla.
        /// </summary>
        public async Task<PokemonUser> ObtenerPokemonAsync(
            int pokemonId, string nombre,
            string tipo1, string tipo2, int currentHp)
        {
            try
            {
                return await ApiClient.PostAsync<PokemonUser>("/pokemon/obtener", new
                {
                    pokemon_id = pokemonId,
                    nombre     = nombre,
                    tipo1      = tipo1,
                    tipo2      = tipo2,
                    current_hp = currentHp
                });
            }
            catch
            {
                return null;
            }
        }

        public void AplicarMovimiento(PokemonUser pokemon, int indiceABorrar, string movimientoNuevo)
        {
            ApiClient.Put<object>("/pokemon/movimiento", new
            {
                pokemon_id       = pokemon.PokemonId,
                indice_a_borrar  = indiceABorrar,
                movimiento_nuevo = movimientoNuevo
            });
        }

        public int ContarPorTipo(string userId, string tipo)
        {
            var lista = ApiClient.Get<List<PokemonUser>>($"/usuarios/{userId}/pokemon");
            return lista.FindAll(p => p.TipoPrincipal == tipo).Count;
        }
    }
}
