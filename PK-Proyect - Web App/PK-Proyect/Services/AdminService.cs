using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;

namespace PK_Proyect.Services
{
    public class AdminService
    {
        private readonly AdminRepository _adminRepo = new();

        public List<User> GetAllUsers()
            => _adminRepo.GetAllUsers();

        public void DeleteUser(string id)
            => _adminRepo.DeleteUser(id);

        public void ChangeRole(User user, string newRole)
            => _adminRepo.ChangeRole(user.Id, newRole);

        /// <summary>
        /// El servidor Flask genera la contraseña y envía el email.
        /// Solo necesitamos llamar al endpoint con password vacío.
        /// </summary>
        public void ResetPassword(User user)
            => ApiClient.Put<object>($"/usuarios/{user.Id}/reset_password", new { });

        public void UpdateUser(User user)
            => _adminRepo.UpdateUser(user);
    }
}
