using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace PK_Proyect.ViewModels
{
    public class PerfilUserViewModel : INotifyPropertyChanged
    {
        // ============================
        // PROPIEDADES NOTIFICABLES
        // ============================

        private string _id;
        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _apellido;
        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _correo;
        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        private string _birthdate;
        public string Birthdate
        {
            get => _birthdate;
            set { _birthdate = value; OnPropertyChanged(); }
        }

        private int _pokes;
        public int Pokes
        {
            get => _pokes;
            set { _pokes = value; OnPropertyChanged(); }
        }

        private int _fichasCasino;
        public int FichasCasino
        {
            get => _fichasCasino;
            set { _fichasCasino = value; OnPropertyChanged(); }
        }

        // ============================
        // CONSTRUCTOR
        // ============================

        public PerfilUserViewModel(User user, UserService userService)
        {
            Id           = "Cargando...";
            Nombre       = "Cargando...";
            Apellido     = "Cargando...";
            Username     = "Cargando...";
            Correo       = "Cargando...";
            Birthdate    = "Cargando...";
            Pokes        = 0;
            FichasCasino = 0;

            _ = CargarPerfilAsync(user, userService);
        }

        // ============================
        // CARGA ASÍNCRONA
        // ============================

        private async Task CargarPerfilAsync(User user, UserService userService)
        {
            try
            {
                var refreshedUser = await Task.Run(() => userService.GetUserById(user.Id));

                if (refreshedUser == null) return;

                // Volver al hilo UI para asignar propiedades
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Id       = user.Id;
                    Nombre   = refreshedUser.Nombre;
                    Apellido = refreshedUser.Apellido;
                    Username = refreshedUser.Username;
                    Correo   = refreshedUser.Correo;

                    Birthdate = (refreshedUser.Birthdate != default)
                        ? refreshedUser.Birthdate.ToString("dd/MM/yyyy")
                        : "No especificada";

                    Pokes        = refreshedUser.Pokes;
                    FichasCasino = refreshedUser.FichasCasino;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el perfil: {ex.Message}");
            }
        }

        // ============================
        // INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
