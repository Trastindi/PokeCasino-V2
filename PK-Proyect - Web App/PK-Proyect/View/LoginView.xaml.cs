using PK_Proyect.Models;
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

            _vm = new LoginViewModel(new AuthService());
            DataContext = _vm;
            _vm.LoginSuccess += OnLoginSuccess;
        }

        private void OnLoginSuccess(User usuario)
        {
            if (usuario.Role == "admin")
                new AdminMenuView(usuario, new AdminService()).Show();
            else
                new MainMenuView(usuario, new UserService()).Show();

            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => _vm.Password = ((PasswordBox)sender).Password;

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
            => new ForgotPasswordView().ShowDialog();
    }
}
