using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    [BsonIgnoreExtraElements]
    public class PokemonUser
    {
        [BsonElement("numero_pokedex")]
        [JsonPropertyName("numero_pokedex")]
        public int numero_pokedex { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("PokemonId")]
        public int PokemonId { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("TipoPrincipal")]
        public string TipoPrincipal { get; set; }

        [JsonPropertyName("TipoSecundario")]
        public string TipoSecundario { get; set; }

        [JsonPropertyName("Nivel")]
        public int Nivel { get; set; } = 1;

        [JsonPropertyName("Cantidad")]
        public int Cantidad { get; set; } = 1;

        [JsonPropertyName("FechaObtenido")]
        public DateTime FechaObtenido { get; set; }

        [JsonPropertyName("AbilityId")]
        public string AbilityId { get; set; } = null;

        [JsonPropertyName("ItemId")]
        public string ItemId { get; set; } = null;

        [JsonPropertyName("MoveSet")]
        public List<string> MoveSet { get; set; } = new();

        [JsonPropertyName("CurrentHp")]
        public int CurrentHp { get; set; } = 0;

        [JsonPropertyName("Status")]
        public string Status { get; set; } = null;

        [JsonPropertyName("HiddenPowerSeed")]
        public int HiddenPowerSeed { get; set; } = 0;

        [JsonPropertyName("HiddenPowerPower")]
        public int HiddenPowerPower { get; set; } = 0;

        /// <summary>
        /// Moneda de fragmentos. Reservado para uso futuro.
        /// Se obtiene acumulando Pokémon duplicados u otras acciones.
        /// </summary>
        [JsonPropertyName("Shards")]
        public int Shards { get; set; } = 0;
    }
}
