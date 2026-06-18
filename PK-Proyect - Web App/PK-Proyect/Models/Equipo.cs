using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Representa un equipo Pokémon de un usuario.
    /// Los integrantes son los _id (ObjectId como string) de los documentos PokemonUser,
    /// NO el número de la Pokédex (PokemonId).
    /// </summary>
    public class Equipo
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("team_name")]
        public string Nombre { get; set; }

        /// <summary>
        /// Lista de _id (ObjectId string) de documentos PokemonUser que forman el equipo.
        /// </summary>
        [JsonPropertyName("pokemon_ids")]
        public List<string> PokemonIds { get; set; } = new List<string>();

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }
}
