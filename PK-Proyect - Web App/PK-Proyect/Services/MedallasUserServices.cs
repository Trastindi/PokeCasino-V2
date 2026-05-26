using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class MedallasUserService
    {
        private readonly MedallasUserRepository _repo = new();

        public bool OtorgarMedalla(string userId, string tipo)
        {
            try
            {
                _repo.InsertMedalla(new MedallasUser { UserId = userId, Tipo = tipo });
                return true;
            }
            catch { return false; } // 409 = ya la tiene → devuelve false
        }
    }
}
