using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

[BsonIgnoreExtraElements]
public class HistoricoTirada
{
    [JsonPropertyName("user_id")]        public string UserId        { get; set; }
    [JsonPropertyName("pokemon_id")]     public int    PokemonId     { get; set; }
    [JsonPropertyName("nombre_pokemon")] public string NombrePokemon { get; set; }
    [JsonPropertyName("zona")]           public string Zona          { get; set; }
    [JsonPropertyName("tipo_tirada")]    public string TipoTirada    { get; set; }

    // Flask puede devolver la fecha en distintos formatos (ISO 8601, $date BSON, etc.).
    // Se deserializa como string y se parsea localmente para evitar JsonException.
    [JsonPropertyName("fecha")]          public string Fecha         { get; set; }

    /// <summary>Intenta parsear Fecha a DateTime. Devuelve null si el formato es desconocido.</summary>
    [JsonIgnore]
    public DateTime? FechaDateTime
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Fecha)) return null;
            if (DateTime.TryParse(Fecha, System.Globalization.CultureInfo.InvariantCulture,
                                  System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                return dt;
            return null;
        }
    }

    /// <summary>Fecha formateada para mostrar en la UI.</summary>
    [JsonIgnore]
    public string FechaDisplay => FechaDateTime?.ToString("dd/MM/yyyy HH:mm") ?? Fecha ?? "-";
}
