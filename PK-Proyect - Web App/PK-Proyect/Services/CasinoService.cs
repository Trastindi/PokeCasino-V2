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
