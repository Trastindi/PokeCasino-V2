using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Convierte el formato de fecha MongoDB Extended JSON { "$date": "..." }
/// o un string ISO normal en un DateTime?.
/// </summary>
public class MongoDateConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Caso 1: objeto { "$date": "2026-02-27T10:32:56.549Z" }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            string dateValue = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    reader.Read(); // avanzar al valor
                    dateValue = reader.GetString();
                }
            }
            if (DateTime.TryParse(dateValue,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var dt1))
                return dt1;
            return null;
        }

        // Caso 2: string ISO directo "2026-02-27T10:32:56.549Z"
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParse(reader.GetString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var dt2))
                return dt2;
            return null;
        }

        // Null / desconocido
        reader.Skip();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString("o"));
        else
            writer.WriteNullValue();
    }
}

/// <summary>
/// Modelo de una tirada del histórico.
/// Usa PropertyNameCaseInsensitive=true (ya configurado en ApiClient._jsonOptions)
/// por lo que no se necesitan [JsonPropertyName] adicionales.
/// </summary>
public class HistoricoTirada
{
    public string UserId       { get; set; }
    public int    PokemonId    { get; set; }
    public string NombrePokemon { get; set; }
    public string Zona         { get; set; }
    public string TipoTirada   { get; set; }

    /// <summary>
    /// Acepta tanto un objeto { "$date": "..." } como un string ISO.
    /// </summary>
    [JsonConverter(typeof(MongoDateConverter))]
    public DateTime? Fecha { get; set; }

    /// <summary>Fecha formateada para la UI: "18/06/2026 19:25".</summary>
    [JsonIgnore]
    public string FechaDisplay => Fecha?.ToString("dd/MM/yyyy HH:mm") ?? "-";
}
