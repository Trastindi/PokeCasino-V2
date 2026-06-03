using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Text.Json.Serialization;

namespace PK_Proyect.Services
{
    /// <summary>
    /// Llama a POST /auth/login en Flask, guarda el JWT en ApiClient
    /// y devuelve el User deserializado.
    /// </summary>
    public class AuthService
    {
        public User Login(string input, string password)
        {
            try
            {
                // Enviamos tanto email como username: el servidor usa $or y acepta cualquiera
                var resp = ApiClient.Post<LoginResponse>("/auth/login", new
                {
                    email    = input,
                    username = input,
                    password = password
                });

                if (resp?.Token == null)
                    return null;

                ApiClient.SetToken(resp.Token);

                return new User
                {
                    Id           = resp.Id,
                    Username     = resp.Username,
                    Correo       = resp.Email,
                    Role         = resp.Rol,
                    FichasCasino = resp.Fichas,
                    Pokes        = resp.Pokes,   // PokeDólares
                    Pokemon      = resp.Pokemon, // Cantidad total de Pokémon
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
            [JsonPropertyName("pokemon")]  public int    Pokemon  { get; set; }
        }
    }
}
