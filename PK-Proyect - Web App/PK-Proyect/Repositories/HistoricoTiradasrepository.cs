using PK_Proyect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Accede al historial de tiradas a través de la API REST del servidor Flask.
    /// Ya no habla directamente con MongoDB.
    /// </summary>
    public class HistoricoTiradasRepository
    {
        /// <summary>Registra una tirada en el servidor (async).</summary>
        public async Task RegistrarTirada(HistoricoTirada tirada)
            => await ApiClient.PostAsync<object>("/historico", tirada);

        /// <summary>Devuelve todo el historial de un usuario.</summary>
        public List<HistoricoTirada> ObtenerPorUsuario(string userId)
            => ApiClient.Get<List<HistoricoTirada>>($"/historico/{userId}") ?? new List<HistoricoTirada>();
    }
}
