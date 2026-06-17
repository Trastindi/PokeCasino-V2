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
    /// </summary>
    public static class ApiClient
    {
        public static string BaseUrl { get; set; } = "https://pokecasino.dpdns.org";

        /// <summary>ID del usuario autenticado, guardado al hacer login.</summary>
        public static string CurrentUserId { get; private set; }

        private static string _token = null;
        private static readonly HttpClient _http = new HttpClient();

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new FlexibleDateTimeConverter() }
        };

        public static void SetToken(string token, string userId = null)
        {
            _token = token;
            CurrentUserId = userId;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearToken()
        {
            _token = null;
            CurrentUserId = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }

        public static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            if (_token != null)
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            return client;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static StringContent Json(object body)
            => new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

        // ── Asíncronas ───────────────────────────────────────────────────────

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

        // ── Síncronas (compatibilidad WPF) ───────────────────────────────────

        public static T Get<T>(string path)
            => GetAsync<T>(path).GetAwaiter().GetResult();

        public static T Post<T>(string path, object body)
            => PostAsync<T>(path, body).GetAwaiter().GetResult();

        public static T Put<T>(string path, object body)
            => PutAsync<T>(path, body).GetAwaiter().GetResult();

        public static void Delete(string path)
            => DeleteAsync(path).GetAwaiter().GetResult();
    }

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
            "o",
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
