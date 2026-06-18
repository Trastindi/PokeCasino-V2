using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

[BsonIgnoreExtraElements]
public class HistoricoTirada
{
    [JsonPropertyName("user_id")]        public string   UserId        { get; set; }
    [JsonPropertyName("pokemon_id")]     public int      PokemonId     { get; set; }
    [JsonPropertyName("nombre_pokemon")] public string   NombrePokemon { get; set; }
    [JsonPropertyName("zona")]           public string   Zona          { get; set; }
    [JsonPropertyName("tipo_tirada")]    public string   TipoTirada    { get; set; }
    [JsonPropertyName("fecha")]          public DateTime Fecha         { get; set; }
}
