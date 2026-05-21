using PK_Proyect.Models;
using PK_Proyect.Repositories;

using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {
        private readonly MainMenuViewModel _viewModel;

        public MainMenuView(User usuario, UserService userService)
        {
            InitializeComponent();

            _viewModel = new MainMenuViewModel(usuario, userService);
            DataContext = _viewModel;

            _viewModel.CerrarSesionRequested += OnCerrarSesion;
        }

        private void OnCerrarSesion()
        {
            // Crear dependencias para LoginView
            var userRepo = new UserRepository();
            var authService = new AuthService(userRepo);

            var login = new LoginView();
            Application.Current.MainWindow = login;
            login.Show();
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
