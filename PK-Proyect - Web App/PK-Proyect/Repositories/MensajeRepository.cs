using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la colección de mensajes a través del servidor Flask.
    /// </summary>
    public class MensajeRepository
    {
        public List&lt;Mensaje&gt; GetMisMensajes()
            =&gt; ApiClient.Get&lt;List&lt;Mensaje&gt;&gt;("/messages/mis_mensajes");
    }
}
