using PK_Proyect.Models;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class AdminService
    {
        private readonly IAdminRepository _adminRepo;
        private readonly UserService _userService;

        public AdminService(IAdminRepository adminRepo, UserService userService)
        {
            _adminRepo = adminRepo;
            _userService = userService;
        }

        public List<User> GetAllUsers()
        {
            return _adminRepo.GetAllUsers();
        }

        public void DeleteUser(string id)
        {
            _adminRepo.DeleteUser(id);
        }

        public void ChangeRole(User user, string newRole)
        {
            user.Role = newRole;
            _adminRepo.UpdateUser(user);
        }

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

        public void UpdateUser(User user)
        {
            _adminRepo.UpdateUser(user);
        }
    }
}
