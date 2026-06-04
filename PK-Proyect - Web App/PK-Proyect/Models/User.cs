using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Deserializa DateTime aceptando:
    ///   - ISO 8601 plano:             "1990-05-20T00:00:00Z"
    ///   - MongoDB extended JSON v2:   {"$date": "1990-05-20T00:00:00Z"}
    ///   - MongoDB extended JSON v1:   {"$date": {"$numberLong": "643420800000"}}
    /// </summary>
    public class MongoDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out var dt))
                    return dt;
                throw new JsonException($"Formato de fecha no reconocido: {reader.GetString()}");
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                reader.Read();

                DateTime result;
                if (reader.TokenType == JsonTokenType.String)
                {
                    DateTime.TryParse(reader.GetString(), out result);
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    var ms = reader.GetInt64();
                    result = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Read();
                    reader.Read();
                    long ms = reader.TokenType == JsonTokenType.String
                        ? long.Parse(reader.GetString()!)
                        : reader.GetInt64();
                    result = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                    reader.Read();
                }
                else
                {
                    throw new JsonException("Formato de $date no reconocido.");
                }

                reader.Read();
                return result;
            }

            throw new JsonException($"Token inesperado al deserializar DateTime: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    public class UserMessage
    {
        public int      MessageId { get; set; }
        public int      ForeignId { get; set; }
        public string   Text      { get; set; }

        [JsonConverter(typeof(MongoDateTimeConverter))]
        public DateTime Date      { get; set; }

        public bool Read { get; set; }
    }

    public class User
    {
        // El servidor devuelve "_id" (serializado como "id" por _serialize)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("Nombre")]
        public string Nombre   { get; set; }

        [JsonPropertyName("Apellido")]
        public string Apellido { get; set; }

        [JsonPropertyName("Username")]
        public string Username { get; set; }

        private string _email;

        // El servidor devuelve "Correo" en GET /usuarios/{id}
        // pero "email" en el login (mapeado manualmente en AuthService)
        [JsonPropertyName("Correo")]
        public string Correo
        {
            get => _email;
            set
            {
                // Protección: no lanzar excepción si el servidor devuelve
                // cadena vacía o null durante la deserialización JSON.
                if (string.IsNullOrWhiteSpace(value))
                {
                    _email = value;
                    return;
                }
                if (!IsValidEmail(value))
                    throw new ArgumentException("Email inválido");
                _email = value;
            }
        }

        /// <summary>Cantidad total de Pokémon del usuario (suma de duplicados).</summary>
        [JsonPropertyName("Pokemon")]
        public int Pokemon { get; set; } = 0;

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonConverter(typeof(MongoDateTimeConverter))]
        [JsonPropertyName("Birthdate")]
        public DateTime Birthdate { get; set; }

        [JsonPropertyName("Role")]
        public string Role { get; set; }

        /// <summary>PokeDólares: moneda del juego.</summary>
        [JsonPropertyName("Pokes")]
        public int Pokes { get; set; } = 0;

        [JsonPropertyName("FichasCasino")]
        public int FichasCasino { get; set; } = 0;

        [JsonPropertyName("Medallas")]
        public List<string>      Medallas { get; set; } = new List<string>();

        [JsonPropertyName("Messages")]
        public List<UserMessage> Messages { get; set; } = new List<UserMessage>();

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}
