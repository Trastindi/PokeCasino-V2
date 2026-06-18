using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Representa un equipo Pokémon de un usuario.
    /// </summary>
    public class Equipo
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("usuario_id")]
        public string UsuarioId { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        /// <summary>Lista de PokemonId (enteros) que forman el equipo.</summary>
        [JsonPropertyName("integrantes")]
        public List<int> Integrantes { get; set; } = new List<int>();
    }
}
