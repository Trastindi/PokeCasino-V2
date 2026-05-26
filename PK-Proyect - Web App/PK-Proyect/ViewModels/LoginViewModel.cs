using PK_Proyect.Models;
using PK_Proyect.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _auth;

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public event Action<User> LoginSuccess;

        public LoginViewModel(AuthService authService)
        {
            _auth = authService;
            // CanExecute deshabilita el botón mientras hay una petición en curso
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Introduce usuario y contraseña.");
                return;
            }

            IsLoading = true;
            try
            {
                // Ejecutar la llamada HTTP en un hilo de fondo para no bloquear el hilo UI
                var usuario = await Task.Run(() => _auth.Login(Username, Password));

                if (usuario == null)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.");
                    return;
                }

                LoginSuccess?.Invoke(usuario);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
