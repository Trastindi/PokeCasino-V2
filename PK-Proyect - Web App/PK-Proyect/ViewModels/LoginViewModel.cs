using PK_Proyect.Models;
using PK_Proyect.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PK_Proyect.Commands;
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

        public ICommand LoginCommand { get; }

        public event Action<User> LoginSuccess;

        public LoginViewModel(AuthService authService)
        {
            _auth = authService;
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object obj)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Introduce usuario y contraseña.");
                return;
            }

            var usuario = _auth.Login(Username, Password);

            if (usuario == null)
            {
                MessageBox.Show("Usuario o contraseña incorrectos.");
                return;
            }

            LoginSuccess?.Invoke(usuario);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
