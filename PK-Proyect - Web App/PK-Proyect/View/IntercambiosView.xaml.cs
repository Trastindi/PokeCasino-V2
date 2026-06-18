using System.Windows;
using System.Windows.Controls;
using PK_Proyect.ViewModels;

namespace PK_Proyect.Views
{
    public partial class IntercambiosView : UserControl
    {
        private IntercambiosViewModel VM => (IntercambiosViewModel)DataContext;

        public IntercambiosView()
        {
            InitializeComponent();
        }

        private async void BtnEnviarSolicitud_Click(object sender, RoutedEventArgs e)
            => await VM.EnviarSolicitudAsync();

        private async void BtnOfrecer_Click(object sender, RoutedEventArgs e)
            => await VM.OfrecerPokemonAsync();

        private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
            => await VM.ConfirmarIntercambioAsync();

        private async void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => await VM.CancelarIntercambioAsync();

        private async void BtnVerIntercambio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tradeId)
                await VM.CargarMisIntercambiosAsync(); // recarga + muestra
        }
    }
}
