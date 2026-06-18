using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

/// <summary>
/// Modelo de una tirada del historial.
/// Se aceptan múltiples nombres de clave JSON para cubrir cualquier
/// convención que use el servidor Flask (snake_case, PascalCase, camelCase).
/// </summary>
[BsonIgnoreExtraElements]
public class HistoricoTirada
{
    // --- user_id / UserId / userId ---
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    // --- pokemon_id / PokemonId / pokemonId ---
    [JsonPropertyName("pokemon_id")]
    public int PokemonId { get; set; }

    // --- nombre_pokemon / NombrePokemon / nombrePokemon ---
    // Flask puede devolver cualquiera de estos nombres; el primero es el
    // que usa [JsonPropertyName], pero añadimos un setter alternativo
    // mediante una propiedad extra para los otros casings.
    [JsonPropertyName("nombre_pokemon")]
    public string NombrePokemon { get; set; }

    [JsonPropertyName("NombrePokemon")]
    public string NombrePokemonPascal
    {
        get => NombrePokemon;
        set { if (string.IsNullOrEmpty(NombrePokemon)) NombrePokemon = value; }
    }

    [JsonPropertyName("nombrePokemon")]
    public string NombrePokemonCamel
    {
        get => NombrePokemon;
        set { if (string.IsNullOrEmpty(NombrePokemon)) NombrePokemon = value; }
    }

    // --- zona / Zona ---
    [JsonPropertyName("zona")]
    public string Zona { get; set; }

    [JsonPropertyName("Zona")]
    public string ZonaPascal
    {
        get => Zona;
        set { if (string.IsNullOrEmpty(Zona)) Zona = value; }
    }

    // --- tipo_tirada / TipoTirada / tipoTirada ---
    [JsonPropertyName("tipo_tirada")]
    public string TipoTirada { get; set; }

    [JsonPropertyName("TipoTirada")]
    public string TipoTiradaPascal
    {
        get => TipoTirada;
        set { if (string.IsNullOrEmpty(TipoTirada)) TipoTirada = value; }
    }

    [JsonPropertyName("tipoTirada")]
    public string TipoTiradaCamel
    {
        get => TipoTirada;
        set { if (string.IsNullOrEmpty(TipoTirada)) TipoTirada = value; }
    }

    // --- fecha / Fecha ---
    // Se guarda como string para evitar JsonException con formatos no estándar.
    [JsonPropertyName("fecha")]
    public string Fecha { get; set; }

    [JsonPropertyName("Fecha")]
    public string FechaPascal
    {
        get => Fecha;
        set { if (string.IsNullOrEmpty(Fecha)) Fecha = value; }
    }

    /// <summary>Parsea Fecha a DateTime de forma segura. Null si el formato es desconocido.</summary>
    [JsonIgnore]
    public DateTime? FechaDateTime
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Fecha)) return null;
            if (DateTime.TryParse(Fecha,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var dt))
                return dt;
            return null;
        }
    }

    /// <summary>Fecha formateada para la UI: "18/06/2026 19:25". Si no parsea, muestra el string crudo.</summary>
    [JsonIgnore]
    public string FechaDisplay => FechaDateTime?.ToString("dd/MM/yyyy HH:mm") ?? Fecha ?? "-";
}
