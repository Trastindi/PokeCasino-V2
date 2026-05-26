using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {
        private readonly MainMenuViewModel _vm;

        public MainMenuView(User usuario, UserService userService)
        {
            InitializeComponent();
            _vm = new MainMenuViewModel(usuario, userService);
            DataContext = _vm;
            _vm.CerrarSesionRequested += OnCerrarSesion;
        }

        private void OnCerrarSesion()
        {
            new LoginView().Show();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
