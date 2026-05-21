using PK_Proyect.Models;
using PK_Proyect.Repositories;

using System.Printing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace PK_Proyect.Services
{
    public class LoginService
    {
        
        private readonly UserRepository _repoUser = new();

        
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
