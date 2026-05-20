using MongoDB.Driver;
using PK_Proyect.Models;

namespace PK_Proyect.Services
{
    public class AuthService
    {
        private readonly UserService _userService = new UserService();

        public UserService UserService => _userService; // <-- AÑADIDO

        public User Login(string input, string password)
        {
            
            var user = _userService.GetUserByUsername(input);

            
            if (user == null)
                user = _userService.GetUserByEmail(input);

          
            if (user == null)
                return null;

            
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            return user;
        }

    }
}
