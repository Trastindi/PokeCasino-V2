using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User Login(string input, string password)
        {
            // Buscar por username
            var user = _userRepository.GetUserByUsername(input);

            // Si no existe, buscar por email
            if (user == null)
                user = _userRepository.GetUserByEmail(input);

            // Si sigue sin existir → login inválido
            if (user == null)
                return null;

            // Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }
    }
}
