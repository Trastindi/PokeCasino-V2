using PK_Proyect.Models;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class MainMenuView : Window
    {

        private User _usuarioConectado;
        public MainMenuView(User usuario)
        {
            InitializeComponent();
            _usuarioConectado = usuario;


            txtUsuarioConectado.Text = $"Conectado: {_usuarioConectado.Username}";
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

        private void btnCasino_Click(object sender, RoutedEventArgs e)
        {
           
            var Casino = new SlotMachineView();
            Casino.Show();
            // (Application.Current.MainWindow as MainWindow).Navigate(new CasinoView());
        }

        private void btnMapa_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir Mapa (pendiente)");
        }

        private void btnEquipo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir Equipo Pokémon (pendiente)");
        }

        private void btnMedallas_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir Medallas (pendiente)");
        }

        private void btnPerfil_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Abrir Perfil (pendiente)");
        }


        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;


            var login = new LoginView();


            Application.Current.MainWindow = login;


            login.Show();


            //SessionManager.Logout();

            this.Close();
        }


    }
}
