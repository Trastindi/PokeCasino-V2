using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class PasswordService
    {
        /// <summary>Recupera contraseña olvidada → llama a POST /auth/recuperar_password.</summary>
        public bool ForgotPassword(string email, string username)
        {
            try
            {
                ApiClient.Post<object>("/auth/recuperar_password", new
                {
                    email    = email,
                    username = username
                });
                return true;
            }
            catch { return false; }
        }

        /// <summary>Cambia la contraseña del usuario autenticado → llama a POST /auth/cambiar_password.</summary>
        public bool ChangePassword(string passwordActual, string nuevaPassword)
        {
            try
            {
                ApiClient.Post<object>("/auth/cambiar_password", new
                {
                    password_actual = passwordActual,
                    nueva_password  = nuevaPassword
                });
                return true;
            }
            catch { return false; }
        }
    }
}
