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

        // Constructor principal: recibe el usuario ya autenticado
        public MainMenuView(User usuario)
        {
            InitializeComponent();
            _vm = new MainMenuViewModel(usuario, new UserService());
            DataContext = _vm;
            _vm.CerrarSesionRequested += OnCerrarSesion;
        }

        private void OnCerrarSesion()
        {
            new LoginView().Show();
            Close();
        }

        // ── Event handlers referenciados desde el XAML ──────────────────────
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        // Botón "Buscar Combate" tiene también Click="Button_Click" en el XAML
        private void Button_Click(object sender, RoutedEventArgs e) { /* gestionado por AbrirBatallaCommand */ }
    }
}
