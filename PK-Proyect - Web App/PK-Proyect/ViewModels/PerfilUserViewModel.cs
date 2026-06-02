using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.ViewModels
{
    public class PerfilUserViewModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Username { get; set; }
        public string Correo { get; set; }
        public string Birthdate { get; set; }
        public int Pokes { get; set; }
        public int FichasCasino { get; set; }

        public PerfilUserViewModel(User user, UserService userService)
        {
            Id = "Cargando...";
            Nombre = "Cargando...";
            Apellido = "Cargando...";
            Username = "Cargando...";
            Correo = "Cargando...";
            Birthdate = "Cargando...";
            Pokes = 0;
            FichasCasino = 0;

            // Cargar datos de forma asincrónica sin bloquear UI
            _ = CargarPerfilAsync(user, userService);
        }

        private async Task CargarPerfilAsync(User user, UserService userService)
        {
            try
            {
                var refreshedUser = await Task.Run(() => userService.GetUserById(user.Id));

                if (refreshedUser != null)
                {
                    Id = refreshedUser.Id;
                    Nombre = refreshedUser.Nombre;
                    Apellido = refreshedUser.Apellido;
                    Username = refreshedUser.Username;
                    Correo = refreshedUser.Correo;
                    
                    // Manejar el formato de fecha de forma segura
                    try
                    {
                        if (refreshedUser.Birthdate != null && refreshedUser.Birthdate != DateTime.MinValue)
                            Birthdate = refreshedUser.Birthdate.ToString("dd/MM/yyyy");
                        else
                            Birthdate = "No especificada";
                    }
                    catch
                    {
                        Birthdate = "Formato inválido";
                    }
                    
                    Pokes = refreshedUser.Pokes;
                    FichasCasino = refreshedUser.FichasCasino;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el perfil: {ex.Message}");
            }
        }
    }
}
