using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a medallas a través del servidor Flask.
    /// </summary>
    public class MedallasUserRepository
    {
        public List<MedallasUser> GetByUser(string userId)
            => ApiClient.Get<List<MedallasUser>>("/medallas");

        public MedallasUser GetMedalla(string userId, string tipo)
        {
            var lista = GetByUser(userId);
            return lista.Find(m => m.Tipo == tipo);
        }

        public void InsertMedalla(MedallasUser medalla)
            => ApiClient.Post<object>("/medallas/otorgar", new { tipo = medalla.Tipo });
    }
}
