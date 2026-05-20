using PK_Proyect.Models;
using PK_Proyect.Services;

namespace PK_Proyect.ViewModels
{
    public class PerfilUserViewModel
    {
        public string Id { get; }
        public string Nombre { get; }
        public string Apellido { get; }
        public string Username { get; }
        public string Correo { get; }
        public String Birthdate { get; }
        public int Pokes { get; }
        public int FichasCasino { get; }

        public PerfilUserViewModel(User user)
        {
            var userService = new UserService();
            var refreshedUser = userService.GetUserById(user.Id);

            Id = refreshedUser.Id.ToString();
            Nombre = refreshedUser.Nombre;
            Apellido = refreshedUser.Apellido;
            Username = refreshedUser.Username;
            Correo = refreshedUser.Correo;
            Birthdate = refreshedUser.Birthdate.ToString("dd/MM/yyyy");
            Pokes = refreshedUser.Pokes;
            FichasCasino = refreshedUser.FichasCasino;
        }

    }
}
