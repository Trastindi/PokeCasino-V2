using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Text.Json.Serialization;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Registra un nuevo entrenador llamando a POST /auth/register en Flask.
    /// El hash de contraseña lo gestiona el servidor; NO se hace BCrypt aquí.
    /// </summary>
    public class TrainerService
    {
        /// <summary>
        /// Envía los datos del usuario al servidor y devuelve el User creado
        /// (con Id, Token, etc.) listo para navegar a MainMenuView.
        /// Devuelve null si el registro falla (username/email duplicado, error de red).
        /// </summary>
        public User CreateUser(User user)
        {
            try
            {
                var resp = ApiClient.Post<RegisterResponse>("/auth/register", new
                {
                    username  = user.Username,
                    password  = user.Password,
                    email     = user.Correo,
                    nombre    = user.Nombre,
                    apellido  = user.Apellido,
                    birthdate = user.Birthdate.ToString("yyyy-MM-dd")
                });

                if (resp?.Token == null)
                    return null;

                ApiClient.SetToken(resp.Token);

                return new User
                {
                    Id           = resp.Id,
                    Username     = resp.Username,
                    Correo       = resp.Email,
                    Nombre       = user.Nombre,
                    Apellido     = user.Apellido,
                    Birthdate    = user.Birthdate,
                    Role         = resp.Rol,
                    Pokes        = resp.Pokes,
                    FichasCasino = resp.FichasCasino,
                    Pokemon      = resp.Pokemon
                };
            }
            catch
            {
                return null;
            }
        }

        private class RegisterResponse
        {
            [JsonPropertyName("token")]        public string Token        { get; set; }
            [JsonPropertyName("id")]           public string Id           { get; set; }
            [JsonPropertyName("username")]     public string Username     { get; set; }
            [JsonPropertyName("email")]        public string Email        { get; set; }
            [JsonPropertyName("rol")]          public string Rol          { get; set; }
            [JsonPropertyName("pokes")]        public int    Pokes        { get; set; }
            [JsonPropertyName("fichas")]       public int    FichasCasino { get; set; }
            [JsonPropertyName("pokemon")]      public int    Pokemon      { get; set; }
        }
    }
}
