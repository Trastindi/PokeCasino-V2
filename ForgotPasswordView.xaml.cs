using PK_Proyect.Services;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class ForgotPasswordView : Window
    {
        private readonly PasswordService _passwordService = new PasswordService();

        public ForgotPasswordView()
        {
            InitializeComponent();
        }

        private void BtnRecuperar_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();
            string username = TxtUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Debes introducir el correo y el nombre de usuario.",
                                "Datos incompletos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            string nuevaPass = _passwordService.ForgotPassword(email, username);

            if (nuevaPass == null)
            {
                MessageBox.Show("Los datos no coinciden con ninguna cuenta.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Se ha enviado una nueva contraseña a tu correo.",
                            "Contraseña restablecida",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
