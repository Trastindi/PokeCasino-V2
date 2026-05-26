using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PK_Proyect.Services
{
    public class AuthService
    {
        /// <summary>
        /// Llama a POST /auth/login en Flask, guarda el JWT en ApiClient
        /// y devuelve el User deserializado.
        /// </summary>
        public User Login(string input, string password)
        {
            try
            {
                var resp = ApiClient.Post<LoginResponse>("/auth/login", new
                {
                    email    = input,
                    password = password
                });

                if (resp?.Token == null)
                    return null;

                ApiClient.SetToken(resp.Token);

                return new User
                {
                    Id            = resp.Id,
                    Username      = resp.Username,
                    Correo        = resp.Email,
                    Role          = resp.Rol,
                    FichasCasino  = resp.Fichas,
                    Pokemon       = resp.Pokes
                };
            }
            catch
            {
                return null;
            }
        }

        private class LoginResponse
        {
            [JsonPropertyName("token")]    public string Token    { get; set; }
            [JsonPropertyName("id")]       public string Id       { get; set; }
            [JsonPropertyName("username")] public string Username { get; set; }
            [JsonPropertyName("email")]    public string Email    { get; set; }
            [JsonPropertyName("rol")]      public string Rol      { get; set; }
            [JsonPropertyName("fichas")]   public int    Fichas   { get; set; }
            [JsonPropertyName("pokes")]    public int    Pokes    { get; set; }
        }
    }
}
