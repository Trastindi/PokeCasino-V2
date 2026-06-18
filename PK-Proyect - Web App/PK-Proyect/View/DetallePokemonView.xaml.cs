using System.Windows;
using System.Windows.Input;
using PK_Proyect.Models;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class DetallePokemonView : Window
    {
        /// <summary>
        /// Constructor estándar: el ViewModel se encarga de la lógica.
        /// </summary>
        public DetallePokemonView(PokemonUser pokemon)
        {
            InitializeComponent();
            DataContext = new DetallePokemonViewModel(pokemon);
        }

        /// <summary>
        /// Constructor rápido: permite pasar un equipoId en Tag para que el comando lo reciba como parámetro.
        /// </summary>
        public DetallePokemonView(PokemonUser pokemon, string equipoId)
        {
            InitializeComponent();
            DataContext = new DetallePokemonViewModel(pokemon);
            this.Tag = equipoId;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
