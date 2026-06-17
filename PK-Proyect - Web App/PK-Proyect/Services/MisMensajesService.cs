using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Capa de servicio para los mensajes del usuario.
    /// Delega la comunicación HTTP al MensajeRepository.
    /// </summary>
    public class MisMensajesService
    {
        private readonly MensajeRepository _repo;

        public MisMensajesService()
        {
            _repo = new MensajeRepository();
        }

        /// <summary>
        /// Recupera todos los mensajes recibidos por el usuario autenticado,
        /// ordenados del más reciente al más antiguo.
        /// </summary>
        public List<Mensaje> ObtenerMisMensajes()
            => _repo.GetMisMensajes();
    }
}
