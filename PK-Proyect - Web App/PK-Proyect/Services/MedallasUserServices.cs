using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class MedallasUserService
    {
        private readonly MedallasUserRepository _repo;

        public MedallasUserService()
        {
            _repo = new MedallasUserRepository();
        }

        public bool OtorgarMedalla(string userId, string tipo)
        {
            var existente = _repo.GetMedalla(userId, tipo);

            if (existente != null)
                return false; // Ya la tiene

            var nueva = new MedallasUser
            {
                UserId = userId,
                Tipo = tipo,
                Fecha = DateTime.Now
            };

            _repo.InsertMedalla(nueva);
            return true;
        }
    }
}

// NO SE USA ACTUALMENTE