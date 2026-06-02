using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class PerfilUserViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly string _userId;

        private string _id;
        public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _nombre;
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }

        private string _apellido;
        public string Apellido { get => _apellido; set { _apellido = value; OnPropertyChanged(); } }

        private string _username;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private string _correo;
        public string Correo { get => _correo; set { _correo = value; OnPropertyChanged(); } }

        private string _birthdate;
        public string Birthdate { get => _birthdate; set { _birthdate = value; OnPropertyChanged(); } }

        private int _pokes;
        public int Pokes { get => _pokes; set { _pokes = value; OnPropertyChanged(); } }

        private int _fichasCasino;
        public int FichasCasino { get => _fichasCasino; set { _fichasCasino = value; OnPropertyChanged(); } }

        public PerfilUserViewModel(User user, UserService userService)
        {
            _userService = userService;
            _userId = user.Id;

            Id = user.Id;
            Nombre = user.Nombre;
            Apellido = user.Apellido;
            Username = user.Username;
            Correo = user.Correo;
            Birthdate = user.Birthdate == default ? string.Empty : user.Birthdate.ToString("dd/MM/yyyy");
            Pokes = user.Pokes;
            FichasCasino = user.FichasCasino;

            _ = CargarDatosActualizadosAsync();
        }

        private async Task CargarDatosActualizadosAsync()
        {
            try
            {
                var refreshedUser = await Task.Run(() => _userService.GetUserById(_userId));
                if (refreshedUser == null) return;

                Id = refreshedUser.Id;
                Nombre = refreshedUser.Nombre;
                Apellido = refreshedUser.Apellido;
                Username = refreshedUser.Username;
                Correo = refreshedUser.Correo;
                Birthdate = refreshedUser.Birthdate.ToString("dd/MM/yyyy");
                Pokes = refreshedUser.Pokes;
                FichasCasino = refreshedUser.FichasCasino;
            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
