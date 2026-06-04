using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

[BsonIgnoreExtraElements]
public class HistoricoTirada
{
    [JsonPropertyName("UserId")]        public string UserId        { get; set; }
    [JsonPropertyName("PokemonId")]     public int    PokemonId     { get; set; }
    [JsonPropertyName("NombrePokemon")] public string NombrePokemon { get; set; }
    [JsonPropertyName("Zona")]          public string Zona          { get; set; }
    [JsonPropertyName("TipoTirada")]    public string TipoTirada    { get; set; }
    [JsonPropertyName("Fecha")]         public DateTime Fecha       { get; set; }
}
