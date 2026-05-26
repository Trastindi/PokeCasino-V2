using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PK_Proyect.Services
{
    public class CasinoService
    {
        /// <summary>
        /// Envía el tablero al servidor y recibe el resultado validado.
        /// El servidor descuenta/suma fichas directamente en BD.
        /// </summary>
        public CasinoResultado Jugar(List<List<int>> tablero, int apuesta)
        {
            try
            {
                return ApiClient.Post<CasinoResultado>("/casino/jugar", new
                {
                    tablero = tablero,
                    apuesta = apuesta
                });
            }
            catch { return null; }
        }

        /// <summary>
        /// Actualiza las fichas del usuario directamente llamando al endpoint de modificar usuario.
        /// Usado por SlotMachineView tras cada tirada.
        /// </summary>
        public void ActualizarFichas(string userId, int fichas)
        {
            try
            {
                ApiClient.Put<object>($"/usuarios/{userId}", new { fichas = fichas });
            }
            catch { /* silencioso, las fichas se actualizan igualmente en /casino/jugar */ }
        }
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
