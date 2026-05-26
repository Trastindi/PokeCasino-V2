using PK_Proyect.Models;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Delega el login en AuthService, que a su vez llama al servidor Flask.
    /// Se mantiene por compatibilidad con el resto del código.
    /// </summary>
    public class LoginService
    {
        private readonly AuthService _auth = new();

        public User Login(string username, string password, string email = null)
        {
            // Intenta primero por username, luego por email si se proporcionó
            var user = _auth.Login(username, password);
            if (user == null && email != null)
                user = _auth.Login(email, password);
            return user;
        }
    }
}
