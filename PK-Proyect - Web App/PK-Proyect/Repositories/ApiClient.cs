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
        private static readonly HttpClient _http = CreateDefaultClient();

        /// <summary>
        /// FIX CS0117: Expone el HttpClient interno como propiedad pública estática
        /// para que BattleService pueda inyectarlo con ApiClient.Client.
        /// El cliente ya tiene BaseAddress y Authorization configurados.
        /// </summary>
        public static HttpClient Client => _http;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Crea el HttpClient con un User-Agent válido para evitar que Cloudflare
        /// bloquee las peticiones.
        /// </summary>
        private static HttpClient CreateDefaultClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new System.Uri(BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            return client;
        }

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
            client.BaseAddress = new System.Uri(BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            if (_token != null)
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            return client;
        }

        private static StringContent Json(object body)
            => new StringContent(JsonSerializer.Serialize(body, _jsonOptions),
                                 Encoding.UTF8, "application/json");

        public static async System.Threading.Tasks.Task<T> GetAsync<T>(string path)
        {
            var r = await _http.GetAsync(path);
            r.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await r.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public static async System.Threading.Tasks.Task<T> PostAsync<T>(string path, object body)
        {
            var r = await _http.PostAsync(path, Json(body));
            r.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await r.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public static async System.Threading.Tasks.Task<T> PutAsync<T>(string path, object body)
        {
            var r = await _http.PutAsync(path, Json(body));
            r.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<T>(await r.Content.ReadAsStringAsync(), _jsonOptions);
        }

        public static async System.Threading.Tasks.Task DeleteAsync(string path)
        {
            var r = await _http.DeleteAsync(path);
            r.EnsureSuccessStatusCode();
        }

        public static T Get<T>(string path)
            => GetAsync<T>(path).GetAwaiter().GetResult();
        public static T Post<T>(string path, object body)
            => PostAsync<T>(path, body).GetAwaiter().GetResult();
        public static T Put<T>(string path, object body)
            => PutAsync<T>(path, body).GetAwaiter().GetResult();
    }
}
