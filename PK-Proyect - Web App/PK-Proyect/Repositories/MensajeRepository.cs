using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la colección de mensajes a través del servidor Flask.
    /// Endpoint: GET /messages/mis_mensajes  (requiere JWT, filtra por "to" = userId).
    /// </summary>
    public class MensajeRepository
    {
        /// <summary>Devuelve todos los mensajes recibidos por el usuario autenticado.</summary>
        public List<Mensaje> GetMisMensajes()
            => ApiClient.Get<List<Mensaje>>("/messages/mis_mensajes");
    }
}
