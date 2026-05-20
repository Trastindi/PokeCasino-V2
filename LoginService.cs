using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Repository;
using System.Printing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace PK_Proyect.Services
{
    public class LoginService
    {
        
        private readonly UserRepository _repoUser = new();

        /// <summary>
        /// Intenta iniciar sesión con username y password.
        /// Devuelve el usuario si es correcto, o null si no coincide.
        /// </summary>
        public User Login(string username, string password, string email)
        {
            // Buscar usuario en MongoDB
            var user = _repoUser.GetUserByUsername(username);

            if (user == null)
                user = _repoUser.GetUserByEmail(email);

            if (user == null)
                return null;

            // Comparar contraseña hasheada con BCrypt
            bool ok = BCrypt.Net.BCrypt.Verify(password, user.Password);

            return ok ? user : null;
        }
    }
}
