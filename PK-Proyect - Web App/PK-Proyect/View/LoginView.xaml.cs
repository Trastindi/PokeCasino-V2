using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _vm;

        public LoginView()
        {
            InitializeComponent();

            // Crear repositorio y servicio de autenticación
            var userRepo = new UserRepository();
            var authService = new AuthService(userRepo);

            // Inyectar dependencias al ViewModel
            _vm = new LoginViewModel(authService);
            DataContext = _vm;

            _vm.LoginSuccess += OnLoginSuccess;
        }

        private void OnLoginSuccess(User usuario)
        {
            if (usuario.Role == "admin")
            {
                // Crear dependencias para Admin
                var adminRepo = new AdminRepository();
                var userRepo = new UserRepository();
                var userService = new UserService(userRepo);
                var adminService = new AdminService(adminRepo, userService);

                new AdminMenuView(usuario, adminService).Show();
            }
            else
            {
                // Crear dependencias para MainMenu
                var userRepo = new UserRepository();
                var userService = new UserService(userRepo);

                new MainMenuView(usuario, userService).Show();
            }

            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Password = ((PasswordBox)sender).Password;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            var ventana = new ForgotPasswordView();
            ventana.ShowDialog();
        }
    }
}
