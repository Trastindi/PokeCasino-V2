using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Text;

namespace PK_Proyect.Services
{
    public class PasswordService
    {
        private readonly UserRepository _repo = new UserRepository();

        public string ForgotPassword(string email, string username)
        {
            // 1. Buscar usuario por email

            var user = _repo.GetUserByEmail(email);

            if (user == null)
                return null;

            // 2. Validar que el username coincide

            if (!string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
                return null;

            // 3. Generar nueva contraseña

            string nuevaPass = GenerarPasswordAleatoria();

            // 4. Hashear la contraseña

            user.Password = BCrypt.Net.BCrypt.HashPassword(nuevaPass);

            // 5. Guardar en MongoDB

            _repo.UpdateUser(user);

            // 6. Enviar email (simulado)

            EmailService.Send(
                user.Correo,
                "Recuperación de contraseña",
                $"Tu nueva contraseña es: {nuevaPass}"
            );

            return nuevaPass;
        }

        private string GenerarPasswordAleatoria()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rng = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[rng.Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
           
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


        public bool ChangePassword(User user, string currentPassword, string newPassword)
        {
            // 1. Verificar contraseña actual

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
                return false;

            // 2. Hashear nueva contraseña

            string hashed = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // 3. Guardar en BD

            user.Password = hashed;
            _repo.UpdateUser(user);

            return true;
        }


    }
}
