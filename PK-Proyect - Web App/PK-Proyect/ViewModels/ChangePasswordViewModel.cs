using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class ChangePasswordViewModel
    {
        private readonly PasswordService _passwordService = new PasswordService();
        private readonly User _usuario;

        public string PasswordActual    { get; set; }
        public string PasswordNueva     { get; set; }
        public string PasswordConfirmar { get; set; }

        public ICommand CambiarCommand { get; }
        public event Action CerrarVentana;

        public ChangePasswordViewModel(User usuario)
        {
            _usuario       = usuario;
            CambiarCommand = new RelayCommand(_ => CambiarPassword());
        }

        private void CambiarPassword()
        {
            if (string.IsNullOrWhiteSpace(PasswordActual) ||
                string.IsNullOrWhiteSpace(PasswordNueva)  ||
                string.IsNullOrWhiteSpace(PasswordConfirmar))
            {
                MessageBox.Show("Debes completar todos los campos.");
                return;
            }

            if (PasswordNueva != PasswordConfirmar)
            {
                MessageBox.Show("Las contraseñas nuevas no coinciden.");
                return;
            }

            // ChangePassword solo recibe passwordActual + passwordNueva;
            // el usuario autenticado lo identifica el JWT en ApiClient.
            bool ok = _passwordService.ChangePassword(PasswordActual, PasswordNueva);

            if (!ok)
            {
                MessageBox.Show("La contraseña actual es incorrecta o ha ocurrido un error.");
                return;
            }

            MessageBox.Show("Contraseña cambiada correctamente.");
            CerrarVentana?.Invoke();
        }
    }
}
