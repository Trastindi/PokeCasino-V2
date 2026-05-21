using PK_Proyect.Models;
using PK_Proyect.Repositories;

using System.Windows;

namespace PK_Proyect.Views
{
    public partial class ComprarFichasWindow : Window
    {
        private User _usuario;
        private UserRepository _repo;

        public ComprarFichasWindow(User usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _repo = new UserRepository();
        }

        private void Comprar300(object sender, RoutedEventArgs e)
        {
            Comprar(300);
        }

        private void Comprar3000(object sender, RoutedEventArgs e)
        {
            Comprar(3000);
        }

        private void Comprar(int cantidad)
        {
            int precio = cantidad * 40;

            if (_usuario.Pokes < precio)
            {
                MessageBox.Show("No tienes suficientes pokes.");
                return;
            }

            _usuario.Pokes -= precio;
            _usuario.FichasCasino += cantidad;

            _repo.UpdateUser(_usuario);

            MessageBox.Show($"Has comprado {cantidad} fichas.");

            DialogResult = true;
            Close();
        }

        private void Cancelar(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
