using PK_Proyect.Services;
using System.Windows;
using System.Windows.Input;
using PK_Proyect.Commands;

namespace PK_Proyect.ViewModels
{
    public class ForgotPasswordViewModel
    {
        private readonly PasswordService _passwordService = new PasswordService();

        public string Email { get; set; }
        public string Username { get; set; }

        public ICommand RecuperarCommand { get; }
        public ICommand CancelarCommand { get; }

        public event Action CerrarVentanaRequested;

        public ForgotPasswordViewModel()
        {
            RecuperarCommand = new RelayCommand(_ => Recuperar());
            CancelarCommand = new RelayCommand(_ => CerrarVentanaRequested?.Invoke());
        }

        private void Recuperar()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Debes introducir email y nombre de usuario.");
                return;
            }

            string nuevaPass = _passwordService.ForgotPassword(Email, Username);

            if (nuevaPass == null)
            {
                MessageBox.Show("Los datos no coinciden con ninguna cuenta.");
                return;
            }

            MessageBox.Show("Se ha enviado una nueva contraseña a tu correo.");
            CerrarVentanaRequested?.Invoke();
        }
    }
}
