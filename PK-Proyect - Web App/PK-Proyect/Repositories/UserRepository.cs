using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    /// <summary>
    /// Acceso a usuarios a través del servidor Flask. Ya no habla con MongoDB directamente.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        public User GetUserById(string id)
            => ApiClient.Get<User>($"/usuarios/{id}");

        public User GetUserByUsername(string username)
        {
            // El servidor Flask busca por username_lower en /auth/login;
            // para búsquedas de perfil usamos el listado filtrado en cliente.
            var todos = GetAllUsers();
            return todos.Find(u =>
                string.Equals(u.Username, username, System.StringComparison.OrdinalIgnoreCase));
        }

        public User GetUserByEmail(string email)
        {
            var todos = GetAllUsers();
            return todos.Find(u =>
                string.Equals(u.Correo, email, System.StringComparison.OrdinalIgnoreCase));
        }

        public bool Exists(string username)
            => GetUserByUsername(username) != null;

        public void CreateUser(User user)
            => ApiClient.Post<object>("/usuarios", user);

        public void UpdateUser(User user)
            => ApiClient.Put<object>($"/usuarios/{user.Id}", user);

        public List<User> GetAllUsers()
            => ApiClient.Get<List<User>>("/usuarios");
    }
}
