using PK_Proyect.View.Banners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PK_Proyect.Models;

namespace PK_Proyect.View
{
    public partial class MapaKantoView : Window
    {
        private User usuarioActual;

        public MapaKantoView(User usuario)
        {
            InitializeComponent();
            usuarioActual = usuario;
        }

        // ---- Hover tooltip ----

        private void Punto_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border punto)
            {
                HoverLabel.Text = punto.ToolTip?.ToString() ?? "";
                Point pos = punto.TranslatePoint(new Point(0, 0), MapaKanto);
                HoverLabel.Margin = new Thickness(pos.X + 5, pos.Y - 15, 0, 0);
                HoverLabel.Visibility = Visibility.Visible;
            }
        }

        private void Punto_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverLabel.Visibility = Visibility.Collapsed;
        }

        private void CerrarVentana_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // ---- Handler genérico de zona ----
        // Todos los Borders del mapa deben tener Tag="Nombre de la Zona"
        // y apuntar su MouseButtonEvent a AbrirZona_Click.

        private void AbrirZona_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement elemento && elemento.Tag is string zonaNombre)
            {
                GachaView ventana = new GachaView(usuarioActual, zonaNombre);
                ventana.ShowDialog();
            }
        }

        // ---- Handlers legacy (mantenidos mientras el XAML del mapa no se actualice) ----

        private void AbrirLaboratorio_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Pueblo Paleta");

        private void AbrirCiudadVerde_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Verde");

        private void AbrirCiudadPlateada_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Plateada");

        private void AbrirCiudadCeleste_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Celeste");

        private void AbrirCiudadCarmin_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Carmín");

        private void AbrirCiudadAzulona_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Azulona");

        private void AbrirCiudadAzafran_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Azafrán");

        private void AbrirCiudadFucsia_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ciudad Fucsia");

        private void AbrirCiudadCanela_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Isla Canela");

        private void AbrirMesetaAnil_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Meseta Añil");

        private void AbrirRuta1_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 1");

        private void AbrirRuta2_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 2");

        private void AbrirRuta3_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 3");

        private void AbrirRuta4_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 4");

        private void AbrirRuta5_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 5");

        private void AbrirRuta6_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 6");

        private void AbrirRuta7_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 7");

        private void AbrirRuta8_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 8");

        private void AbrirRuta9_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 9");

        private void AbrirRuta10_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 10");

        private void AbrirRuta11_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 11");

        private void AbrirRuta12_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 12");

        private void AbrirRuta13_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 13");

        private void AbrirRuta14_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 14");

        private void AbrirRuta15_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 15");

        private void AbrirRuta16_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 16");

        private void AbrirRuta17_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 17");

        private void AbrirRuta18_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 18");

        private void AbrirRuta19_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 19");

        private void AbrirRuta20_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 20");

        private void AbrirRuta21_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 21");

        private void AbrirRuta22_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 22");

        private void AbrirRuta23_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 23");

        private void AbrirRuta24_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 24");

        private void AbrirRuta25_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Ruta 25");

        private void AbrirBosqueVerde_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Bosque Verde");

        private void AbrirMonteMoon_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Monte Moon");

        private void AbrirTunelRoca_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Túnel Roca");

        private void AbrirTunelDiglett_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Túnel Diglett");

        private void AbrirTorrePokemon_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Torre Pokémon");

        private void AbrirMansionPokemon_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Mansión Pokémon");

        private void AbrirZonaSafari_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Zona Safari");

        private void AbrirIslasEspuma_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Islas Espuma");

        private void AbrirCalleVictoria_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Calle Victoria");

        private void AbrirCentralElectrica_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Central Eléctrica");

        private void AbrirCuevaCeleste_Click(object sender, MouseButtonEventArgs e)
            => AbrirZona("Cueva Celeste");

        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;
            media.Position = TimeSpan.Zero;
            media.Play();
        }

        // ---- Helper privado ----

        private void AbrirZona(string zonaNombre)
        {
            GachaView ventana = new GachaView(usuarioActual, zonaNombre);
            ventana.ShowDialog();
        }
    }
}
