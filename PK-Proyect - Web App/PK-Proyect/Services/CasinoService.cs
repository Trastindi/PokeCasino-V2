using PK_Proyect.Models;
using PK_Proyect.Repositories;   // ApiClient.BaseUrl
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    public class CasinoService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        /// <summary>
        /// Constructor por defecto: reutiliza ApiClient.BaseUrl y crea
        /// un HttpClient propio. Es el que usa MainMenuViewModel.
        /// </summary>
        public CasinoService()
        {
            _baseUrl = ApiClient.BaseUrl;
            _http    = ApiClient.CreateHttpClient();
        }

        /// <summary>
        /// Constructor con inyección explícita (tests / DI manual).
        /// </summary>
        public CasinoService(HttpClient http, string baseUrl)
        {
            _http    = http;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Versión asíncrona — usar siempre desde el UI Thread.
        /// No bloquea el Dispatcher mientras espera la respuesta HTTP.
        /// </summary>
        public async Task<CasinoResultado> JugarAsync(List<List<int>> tablero, int apuesta)
        {
            try
            {
                var payload = new { tablero, apuesta };
                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _http.PostAsync($"{_baseUrl}/casino/jugar", content);
                if (!response.IsSuccessStatusCode) return null;

                string body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CasinoResultado>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }

        /// <summary>
        /// Alias bloqueante para compatibilidad con código heredado.
        /// Preferir JugarAsync() siempre que sea posible.
        /// </summary>
        public CasinoResultado Jugar(List<List<int>> tablero, int apuesta)
            => JugarAsync(tablero, apuesta).GetAwaiter().GetResult();
    }

    public class CasinoResultado
    {
        [JsonPropertyName("tablero")]           public List<List<int>> Tablero         { get; set; }
        [JsonPropertyName("simbolos")]          public List<string>    Simbolos        { get; set; }
        [JsonPropertyName("apuesta")]           public int             Apuesta         { get; set; }
        [JsonPropertyName("payout")]            public int             Payout          { get; set; }
        [JsonPropertyName("lineas_ganadoras")]  public List<string>    LineasGanadoras { get; set; }
        [JsonPropertyName("fichas_final")]      public int             FichasFinal     { get; set; }
    }
}
