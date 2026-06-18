using PK_Proyect.Models;
using PK_Proyect.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.View
{
    public partial class SeleccionarPokemonWindow : Window
    {
        private SeleccionarPokemonViewModel VM => DataContext as SeleccionarPokemonViewModel;

        public SeleccionarPokemonWindow(SeleccionarPokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        // Clic en la tarjeta del Pokémon → seleccionarlo
        private void PokemonCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.DataContext is PokemonUser pokemon)
            {
                VM.PokemonSeleccionado = pokemon;
            }
        }

        // Botón "Ver detalles" → abre DetallePokemonView en modo lectura
        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (VM?.PokemonSeleccionado == null) return;
            var detalle = new DetallePokemonView(VM.PokemonSeleccionado)
            {
                Owner = this
            };
            detalle.ShowDialog();
        }

        // Botón "Elegir este Pokémon" → cierra con DialogResult = true
        private void BtnElegir_Click(object sender, RoutedEventArgs e)
        {
            if (VM?.PokemonSeleccionado != null)
            {
                VM.PokemonElegido = VM.PokemonSeleccionado;
                DialogResult = true;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.TextoBusqueda = string.Empty;
        }
    }
}
