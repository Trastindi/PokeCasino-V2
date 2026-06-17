using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la coleccion de mensajes a traves del servidor Flask.
    /// </summary>
    public class MensajeRepository
    {
        public List<Mensaje> GetMisMensajes()
            => ApiClient.Get<List<Mensaje>>("/messages/mis_mensajes");
    }
}
