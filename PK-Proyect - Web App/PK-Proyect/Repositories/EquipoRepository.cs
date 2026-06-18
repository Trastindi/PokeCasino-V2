using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a la colección Equipos a través del servidor Flask.
    /// Endpoints esperados:
    ///   GET    /usuarios/{userId}/equipos          → List<Equipo>
    ///   POST   /equipos                            → Equipo creado
    ///   DELETE /equipos/{equipoId}                 → 204
    /// </summary>
    public class EquipoRepository
    {
        public List<Equipo> GetEquiposByUser(string userId)
            => ApiClient.Get<List<Equipo>>($"/usuarios/{userId}/equipos");

        public Equipo CrearEquipo(string userId, string nombre)
            => ApiClient.Post<Equipo>("/equipos", new
            {
                usuario_id = userId,
                nombre     = nombre
            });

        public void EliminarEquipo(string equipoId)
            => ApiClient.Delete($"/equipos/{equipoId}");
    }
}
