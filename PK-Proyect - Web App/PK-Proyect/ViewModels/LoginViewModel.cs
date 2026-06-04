using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        // _password sigue existiendo como fallback (PasswordChanged lo actualiza)
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

            // CommandParameter recibe el PasswordBox desde el XAML.
            // Si por alguna razón llega null, se usa _password como fallback.
            LoginCommand = new RelayCommand(
                async param => await LoginAsync(param as PasswordBox),
                _          => !IsLoading
            );
        }

        private async Task LoginAsync(PasswordBox passwordBox)
        {
            // Leer la contraseña en el UI Thread ANTES de cualquier await
            // para garantizar que el valor está actualizado aunque el usuario
            // haya pulsado Enter sin mover el foco del PasswordBox.
            string password = passwordBox?.Password ?? _password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Introduce usuario y contraseña.");
                return;
            }

            IsLoading = true;
            try
            {
                var usuario = await Task.Run(() => _auth.Login(Username, password));

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
