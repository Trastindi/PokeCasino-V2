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
                // Formato plano ISO 8601
                if (DateTime.TryParse(reader.GetString(), out var dt))
                    return dt;
                throw new JsonException($"Formato de fecha no reconocido: {reader.GetString()}");
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // { "$date": ... }
                reader.Read(); // PropertyName "$date"
                reader.Read(); // value

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
                    // { "$numberLong": "643420800000" }
                    reader.Read(); // PropertyName "$numberLong"
                    reader.Read(); // value (string o number)
                    long ms = reader.TokenType == JsonTokenType.String
                        ? long.Parse(reader.GetString()!)
                        : reader.GetInt64();
                    result = DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                    reader.Read(); // EndObject de $numberLong
                }
                else
                {
                    throw new JsonException("Formato de $date no reconocido.");
                }

                reader.Read(); // EndObject de $date
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
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Nombre   { get; set; }
        public string Apellido { get; set; }
        public string Username { get; set; }

        private string _email;

        public string Correo
        {
            get => _email;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Email inválido");
                _email = value;
            }
        }

        /// <summary>Cantidad total de Pokémon del usuario (suma de duplicados).</summary>
        public int Pokemon { get; set; } = 0;

        public string Password { get; set; }

        [JsonConverter(typeof(MongoDateTimeConverter))]
        public DateTime Birthdate { get; set; }

        public string Role         { get; set; }

        /// <summary>PokeDólares: moneda del juego.</summary>
        public int Pokes        { get; set; } = 0;
        public int FichasCasino { get; set; } = 0;

        public List<string>      Medallas { get; set; } = new List<string>();
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
