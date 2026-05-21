using PK_Proyect.ViewModels;
using PK_Proyect.Services;
using PK_Proyect.Models;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class PerfilUserView : Window
    {
        private readonly User _usuarioActual;

        public PerfilUserView(User user, UserService userService)
        {
            InitializeComponent();

            _usuarioActual = user;

            DataContext = new PerfilUserViewModel(user, userService);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ChangePasswordView(_usuarioActual);
            ventana.ShowDialog();
        }
    }
}
