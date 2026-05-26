using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Cliente HTTP centralizado para comunicarse con el servidor Flask.
    /// Todos los repositories usan esta clase en lugar de conectarse directamente a MongoDB.
    /// </summary>
    public static class ApiClient
    {
        // Cambia esta URL si el servidor corre en otra dirección
        public static string BaseUrl { get; set; } = "pokecasino.dpdns.org";

        private static string _token = null;
        private static readonly HttpClient _http = new HttpClient();

        /// <summary>Guarda el JWT tras el login para adjuntarlo en cada petición.</summary>
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

        // ── Helpers ─────────────────────────────────────────────────────────

        private static StringContent Json(object body)
            => new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

        public static async Task<T> GetAsync<T>(string path)
        {
            var resp = await _http.GetAsync(BaseUrl + path);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<T> PostAsync<T>(string path, object body)
        {
            var resp = await _http.PostAsync(BaseUrl + path, Json(body));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }

        public static async Task<T> PutAsync<T>(string path, object body)
        {
            var resp = await _http.PutAsync(BaseUrl + path, Json(body));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }

        public static async Task DeleteAsync(string path)
        {
            var resp = await _http.DeleteAsync(BaseUrl + path);
            resp.EnsureSuccessStatusCode();
        }

        // ── Versiones síncronas (para mantener compatibilidad con el código WPF existente) ──

        public static T Get<T>(string path)
            => GetAsync<T>(path).GetAwaiter().GetResult();

        public static T Post<T>(string path, object body)
            => PostAsync<T>(path, body).GetAwaiter().GetResult();

        public static T Put<T>(string path, object body)
            => PutAsync<T>(path, body).GetAwaiter().GetResult();

        public static void Delete(string path)
            => DeleteAsync(path).GetAwaiter().GetResult();
    }
}
