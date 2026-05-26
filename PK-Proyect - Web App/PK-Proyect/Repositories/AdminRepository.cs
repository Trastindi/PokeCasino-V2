using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Operaciones de administración a través del servidor Flask.
    /// </summary>
    public class AdminRepository : IAdminRepository
    {
        public List<User> GetAllUsers()
            => ApiClient.Get<List<User>>("/usuarios");

        public void DeleteUser(string id)
            => ApiClient.Delete($"/usuarios/{id}");

        public void UpdateUser(User user)
            => ApiClient.Put<object>($"/usuarios/{user.Id}", user);

        public void ChangeRole(string id, string newRole)
            => ApiClient.Put<object>($"/usuarios/{id}", new { rol = newRole });

        public void ResetPassword(string id, string newHashedPassword)
            => ApiClient.Put<object>($"/usuarios/{id}/reset_password", new { password = newHashedPassword });
    }
}
