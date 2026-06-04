using PK_Proyect.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    public class CasinoService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _userId;

        public CasinoService(HttpClient http, string baseUrl, string userId)
        {
            _http    = http;
            _baseUrl = baseUrl;
            _userId  = userId;
        }

        /// <summary>
        /// Versión asíncrona — usar siempre desde el UI Thread.
        /// No bloquea el Dispatcher mientras espera la respuesta HTTP.
        /// </summary>
        public async Task<CasinoResultado> JugarAsync(List<List<int>> tablero, int apuesta)
        {
            var payload = new
            {
                user_id = _userId,
                tablero = tablero,
                apuesta = apuesta
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _http.PostAsync($"{_baseUrl}/casino/jugar", content);
            if (!response.IsSuccessStatusCode) return null;

            string body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CasinoResultado>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Mantiene compatibilidad con código heredado que aún llame a Jugar() síncrono.
        /// Internamente delega en JugarAsync() de forma bloqueante — preferir JugarAsync.
        /// </summary>
        public CasinoResultado Jugar(List<List<int>> tablero, int apuesta)
            => JugarAsync(tablero, apuesta).GetAwaiter().GetResult();
    }
}
