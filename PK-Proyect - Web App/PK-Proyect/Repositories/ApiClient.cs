using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Cliente HTTP centralizado para comunicarse con el servidor Flask.
    /// Todos los services usan esta clase en lugar de conectarse directamente a MongoDB.
    /// </summary>
    public static class ApiClient
    {
        public static string BaseUrl { get; set; } = "https://pokecasino.dpdns.org";

        private static string _token = null;
        private static readonly HttpClient _http = new HttpClient();

        /// <summary>
        /// Opciones JSON globales:
        /// - PropertyNameCaseInsensitive: acepta claves en cualquier casing (Fecha/fecha/FECHA).
        /// - Converters: DateTimeConverter tolera el formato isoformat de Python
        ///   (sin 'Z' ni offset, e.g. "2026-06-04T10:17:46.123456").
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new FlexibleDateTimeConverter() }
        };

        public static void SetToken(string token)
        {
            _token = token;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearToken()
        {
            _token = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>Crea un HttpClient con el JWT ya configurado (para servicios que lo necesiten).</summary>
        public static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            if (_token != null)
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            return client;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static StringContent Json(object body)
            => new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

        // ── Versiones asíncronas ──────────────────────────────────────────────

        public static async System.Threading.Tasks.Task<T> GetAsync<T>(string path)
        {
            var resp = await _http.GetAsync(BaseUrl + path);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public static async System.Threading.Tasks.Task<T> PostAsync<T>(string path, object body)
        {
            var resp = await _http.PostAsync(BaseUrl + path, Json(body));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public static async System.Threading.Tasks.Task<T> PutAsync<T>(string path, object body)
        {
            var resp = await _http.PutAsync(BaseUrl + path, Json(body));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public static async System.Threading.Tasks.Task DeleteAsync(string path)
        {
            var resp = await _http.DeleteAsync(BaseUrl + path);
            resp.EnsureSuccessStatusCode();
        }

        // ── Versiones síncronas (compatibilidad WPF) ──────────────────────────

        public static T Get<T>(string path)
            => GetAsync<T>(path).GetAwaiter().GetResult();

        public static T Post<T>(string path, object body)
            => PostAsync<T>(path, body).GetAwaiter().GetResult();

        public static T Put<T>(string path, object body)
            => PutAsync<T>(path, body).GetAwaiter().GetResult();

        public static void Delete(string path)
            => DeleteAsync(path).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Converter de DateTime flexible: acepta el formato isoformat de Python
    /// ("2026-06-04T10:17:46", "2026-06-04T10:17:46.123456") además
    /// del formato estándar con 'Z' o con offset.
    /// </summary>
    internal class FlexibleDateTimeConverter : JsonConverter<System.DateTime>
    {
        private static readonly string[] _formats =
        {
            "yyyy-MM-ddTHH:mm:ss.ffffff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "o",   // round-trip ISO 8601
        };

        public override System.DateTime Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (System.DateTime.TryParse(s,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var dt))
                return dt;

            foreach (var fmt in _formats)
                if (System.DateTime.TryParseExact(s, fmt,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out dt))
                    return dt;

            throw new JsonException($"Formato de fecha no reconocido: '{s}'");
        }

        public override void Write(
            Utf8JsonWriter writer,
            System.DateTime value,
            JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("o"));
    }
}
