using PK_Proyect;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;
using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.Models;
namespace PK_Proyect.View
{
    public partial class PerfilUserView : Window
    {


        private User _usuarioActual;
        public PerfilUserView(User user)
        {
            InitializeComponent();
            _usuarioActual = user;
            DataContext = new PerfilUserViewModel(user);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
