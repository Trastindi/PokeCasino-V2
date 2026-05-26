using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PK_Proyect.Services
{
    public class PokemonUserService
    {
        /// <summary>
        /// Delega toda la lógica de obtención / subida de nivel / evolución al servidor Flask.
        /// El servidor devuelve un LevelUpResultado serializado.
        /// </summary>
        public LevelUpResultado ObtenerPokemon(
            string userId, int pokemonId, string nombre,
            string tipo1, string tipo2, int currentHp)
        {
            try
            {
                var resp = ApiClient.Post<LevelUpResultadoDto>("/pokemon/obtener", new
                {
                    pokemon_id = pokemonId,
                    nombre     = nombre,
                    tipo1      = tipo1,
                    tipo2      = tipo2,
                    current_hp = currentHp
                });

                return new LevelUpResultado
                {
                    Pokemon                          = resp.Pokemon,
                    MovimientoAprendido              = resp.MovimientoAprendido,
                    MovimientoAprendidoDirectamente  = resp.MovimientoAprendidoDirectamente,
                    MovimientoEvolucion              = resp.MovimientoEvolucion,
                    MovimientoEvolucionDirectamente  = resp.MovimientoEvolucionDirectamente,
                    Evoluciono                       = resp.Evoluciono,
                    NombreEvolucion                  = resp.NombreEvolucion
                };
            }
            catch
            {
                return new LevelUpResultado();
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

        // DTO interno para deserializar la respuesta Flask
        private class LevelUpResultadoDto
        {
            [JsonPropertyName("pokemon")]                           public PokemonUser Pokemon                         { get; set; }
            [JsonPropertyName("movimiento_aprendido")]              public string      MovimientoAprendido             { get; set; }
            [JsonPropertyName("movimiento_aprendido_directamente")] public bool        MovimientoAprendidoDirectamente { get; set; }
            [JsonPropertyName("movimiento_evolucion")]              public string      MovimientoEvolucion             { get; set; }
            [JsonPropertyName("movimiento_evolucion_directamente")] public bool        MovimientoEvolucionDirectamente { get; set; }
            [JsonPropertyName("evoluciono")]                        public bool        Evoluciono                      { get; set; }
            [JsonPropertyName("nombre_evolucion")]                  public string      NombreEvolucion                 { get; set; }
        }
    }
}
