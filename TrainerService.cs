using PK_Proyect.Models;
using PK_Proyect.Repositories;
using BCrypt.Net;

namespace PK_Proyect.Services
{
    public class TrainerService
    {
        private readonly TrainerRepository _repo = new();

        public void CreateUser(User user)
        {
            // Hash de contraseña ANTES de guardar
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _repo.Insert(user);
        }
    }
}
