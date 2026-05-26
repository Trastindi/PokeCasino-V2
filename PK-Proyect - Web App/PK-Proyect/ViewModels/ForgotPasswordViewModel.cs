using PK_Proyect.Services;
using PK_Proyect.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class ForgotPasswordViewModel
    {
        private readonly PasswordService _passwordService = new PasswordService();

        public string Email    { get; set; }
        public string Username { get; set; }

        public ICommand RecuperarCommand { get; }
        public ICommand CancelarCommand  { get; }

        public event Action CerrarVentanaRequested;

        public ForgotPasswordViewModel()
        {
            RecuperarCommand = new RelayCommand(_ => Recuperar());
            CancelarCommand  = new RelayCommand(_ => CerrarVentanaRequested?.Invoke());
        }

        private void Recuperar()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Debes introducir email y nombre de usuario.");
                return;
            }

            // ForgotPassword ahora devuelve bool (true = éxito, false = error)
            bool ok = _passwordService.ForgotPassword(Email, Username);

            if (!ok)
            {
                MessageBox.Show("Los datos no coinciden con ninguna cuenta o ha ocurrido un error.");
                return;
            }

            MessageBox.Show("Se ha enviado una nueva contraseña a tu correo.");
            CerrarVentanaRequested?.Invoke();
        }
    }
}
