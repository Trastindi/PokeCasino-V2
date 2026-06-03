using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PK_Proyect.Commands;

namespace PK_Proyect.ViewModels
{
    public class TrainerDataViewModel : INotifyPropertyChanged
    {
        private readonly TrainerService _service = new();

        public event Action CloseRequested;
        public event Action<User> NavigateToMainMenuRequested;

        public TrainerDataViewModel(string username)
        {
            Username = username;
            GuardarCommand  = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        // ============================
        // PROPIEDADES
        // ============================

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

        private DateTime? _birthdate;
        public DateTime? Birthdate
        {
            get => _birthdate;
            set { _birthdate = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _passwordConfirm;
        public string PasswordConfirm
        {
            get => _passwordConfirm;
            set { _passwordConfirm = value; OnPropertyChanged(); }
        }

        // ============================
        // COMANDOS
        // ============================

        public ICommand GuardarCommand  { get; }
        public ICommand CancelarCommand { get; }

        // ============================
        // LÓGICA
        // ============================

        private void Guardar(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre)    ||
                string.IsNullOrWhiteSpace(Apellido)  ||
                string.IsNullOrWhiteSpace(Correo)    ||
                Birthdate == null                     ||
                string.IsNullOrWhiteSpace(Password)  ||
                string.IsNullOrWhiteSpace(PasswordConfirm))
            {
                System.Windows.MessageBox.Show("Por favor, completa todos los campos.");
                return;
            }

            if (!ValidarPassword(Password))
            {
                System.Windows.MessageBox.Show(
                    "La contraseña debe tener:\n" +
                    "- Mínimo 8 caracteres\n" +
                    "- Mayúscula\n" +
                    "- Minúscula\n" +
                    "- Número\n" +
                    "- Carácter especial");
                return;
            }

            if (Password != PasswordConfirm)
            {
                System.Windows.MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }

            User nuevo = new User
            {
                Nombre    = Nombre,
                Apellido  = Apellido,
                Username  = Username,
                Password  = Password,   // el servidor hace el hash
                Correo    = Correo,
                Birthdate = Birthdate.Value
            };

            User creado = _service.CreateUser(nuevo);

            if (creado == null)
            {
                System.Windows.MessageBox.Show(
                    "No se pudo completar el registro.\n" +
                    "El nombre de usuario o email ya están en uso, o el servidor no está disponible.");
                return;
            }

            // Navegar directamente al menú principal con el usuario ya autenticado
            NavigateToMainMenuRequested?.Invoke(creado);
            CloseRequested?.Invoke();
        }

        private bool ValidarPassword(string pass)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";
            return Regex.IsMatch(pass, pattern);
        }

        // ============================
        // INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
