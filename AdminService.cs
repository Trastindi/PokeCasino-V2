using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class AdminService
    {
        private readonly AdminRepository _adminRepo;
        private readonly UserService _userService;

        public AdminService()
        {
            _adminRepo = new AdminRepository();
            _userService = new UserService();
        }

        // Obtener todos los usuarios
        public List<User> GetAllUsers()
        {
            return _adminRepo.GetAllUsers();
        }

        // Eliminar usuario por ID
        public void DeleteUser(string id)
        {
            _adminRepo.DeleteUser(id);
        }

        // Cambiar rol del usuario
        public void ChangeRole(User user, string newRole)
        {
            user.Role = newRole;
            _adminRepo.UpdateUser(user);
        }

        // Resetear contraseña a "1234"
        public void ResetPassword(User user)
        {
            string newPassword = "1234";
            string hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);

            user.Password = hashed;
            _adminRepo.UpdateUser(user);

            EmailService.Send(
                user.Correo,
                "Contraseña restablecida",
                $"Hola {user.Username}, tu nueva contraseña es: {newPassword}"
            );
        }


        // Actualizar usuario completo
        public void UpdateUser(User user)
        {
            _adminRepo.UpdateUser(user);
        }
    }
}
