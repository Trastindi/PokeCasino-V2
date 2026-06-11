using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Registra un nuevo entrenador llamando a POST /auth/register en Flask.
    /// El hash de contraseña lo gestiona el servidor; NO se hace BCrypt aquí.
    /// </summary>
    public class TrainerService
    {
        /// <summary>
        /// Versión ASÍNCRONA: No bloquea el UI Thread mientras espera la respuesta del servidor.
        /// Esta es la versión preferida para llamadas desde la UI.
        /// </summary>
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                var resp = await ApiClient.PostAsync<RegisterResponse>("/auth/register", new
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

        /// <summary>
        /// Envía los datos del usuario al servidor y devuelve el User creado
        /// (con Id, Token, etc.) listo para navegar a MainMenuView.
        /// Devuelve null si el registro falla (username/email duplicado, error de red).
        /// 
        /// DEPRECATED: Usar CreateUserAsync() en su lugar para evitar bloquear el UI Thread.
        /// </summary>
        public User CreateUser(User user)
            => CreateUserAsync(user).GetAwaiter().GetResult();

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
