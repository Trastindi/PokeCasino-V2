using PK_Proyect.View.Banners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PK_Proyect.Models;
using PK_Proyect.View.Banners.Ciudades;
using PK_Proyect.View.Banners.Lugares;
using PK_Proyect.View.Banners.Rutas;

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






        private void Punto_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border punto)
            {
                HoverLabel.Text = punto.ToolTip?.ToString() ?? "";

                // Convertir la posición del punto al Grid que contiene la etiqueta
                Point pos = punto.TranslatePoint(new Point(0, 0), MapaKanto);

                // Ajustar la posición de la etiqueta
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


        //-----------------CIUDADES-----------------//

        //Faltan por añadir
        
        private void AbrirLaboratorio_Click(object sender, MouseButtonEventArgs e)
        {
            LaboratorioView ventana = new LaboratorioView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadVerde_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadVerdeView ventana = new CiudadVerdeView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadPlateada_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadPlateadaView ventana = new CiudadPlateadaView(usuarioActual);
            ventana.ShowDialog();
        }
       

        private void AbrirCiudadCeleste_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadCelesteView ventana = new CiudadCelesteView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadCarmin_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadCarminView ventana = new CiudadCarminView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadAzulona_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadAzulonaView ventana = new CiudadAzulonaView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadAzafran_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadAzafranView ventana = new CiudadAzafranView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCiudadFucsia_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadFucsiaView ventana = new CiudadFucsiaView(usuarioActual);
            ventana.ShowDialog();
        }


        // cambiar nombre de CiudadCanela a IslaCanela
        private void AbrirCiudadCanela_Click(object sender, MouseButtonEventArgs e)
        {
            CiudadCanelaView ventana = new CiudadCanelaView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirMesetaAnil_Click(object sender, MouseButtonEventArgs e)
        {
            MesetaAnilView ventana = new MesetaAnilView(usuarioActual);
            ventana.ShowDialog();
        }

            
        //-----------------RUTAS-----------------//

        private void AbrirRuta1_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta1View ventana = new Ruta1View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta2_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta2View ventana = new Ruta2View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta3_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta3View ventana = new Ruta3View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta4_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta4View ventana = new Ruta4View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta5_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta5View ventana = new Ruta5View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta6_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta6View ventana = new Ruta6View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta7_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta7View ventana = new Ruta7View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta8_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta8View ventana = new Ruta8View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta9_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta9View ventana = new Ruta9View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta10_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta10View ventana = new Ruta10View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta11_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta11View ventana = new Ruta11View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta12_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta12View ventana = new Ruta12View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta13_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta13View ventana = new Ruta13View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta14_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta14View ventana = new Ruta14View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta15_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta15View ventana = new Ruta15View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta16_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta16View ventana = new Ruta16View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta17_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta17View ventana = new Ruta17View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta18_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta18View ventana = new Ruta18View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta19_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta19View ventana = new Ruta19View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta20_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta20View ventana = new Ruta20View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta21_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta21View ventana = new Ruta21View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta22_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta22View ventana = new Ruta22View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta23_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta23View ventana = new Ruta23View(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirRuta24_Click(object sender, MouseButtonEventArgs e)
        {
            Ruta24View ventana = new Ruta24View(usuarioActual);
            ventana.ShowDialog();
        }
        

        private void AbrirRuta25_Click(object sender, MouseButtonEventArgs e)
        {
           Ruta25View ventana = new Ruta25View(usuarioActual);
           ventana.ShowDialog();
        }

        
        //-----------------LUGARES-----------------//


        private void AbrirBosqueVerde_Click(object sender, MouseButtonEventArgs e)
        {
            BosqueVerdeView ventana = new BosqueVerdeView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirMonteMoon_Click(object sender, MouseButtonEventArgs e)
        {
            MonteMoonView ventana = new MonteMoonView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirTunelRoca_Click(object sender, MouseButtonEventArgs e)
        {
           TunelRocaView ventana = new TunelRocaView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirTunelDiglett_Click(object sender, MouseButtonEventArgs e)
        {
           TunelDiglettView ventana = new TunelDiglettView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirTorrePokemon_Click(object sender, MouseButtonEventArgs e)
        {
           TorrePokemonView ventana = new TorrePokemonView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirMansionPokemon_Click(object sender, MouseButtonEventArgs e)
        {
           MansionPokemonView ventana = new MansionPokemonView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirZonaSafari_Click(object sender, MouseButtonEventArgs e)
        {
           ZonaSafariView ventana = new ZonaSafariView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirIslasEspuma_Click(object sender, MouseButtonEventArgs e)
        {
            IslasEspumaView ventana = new IslasEspumaView(usuarioActual);
            ventana.ShowDialog();
        }

        private void AbrirCalleVictoria_Click(object sender, MouseButtonEventArgs e)
        {
           CalleVictoriaView ventana = new CalleVictoriaView(usuarioActual);
           ventana.ShowDialog();
        }

        private void AbrirCentralElectrica_Click(object sender, MouseButtonEventArgs e)
        {
            CentralElectricaView ventana = new CentralElectricaView(usuarioActual);
            ventana.ShowDialog();
        }


        private void AbrirCuevaCeleste_Click(object sender, MouseButtonEventArgs e)
        {
            CuevaCelesteView ventana = new CuevaCelesteView(usuarioActual);
            ventana.ShowDialog();
        }

        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;

            media.Position = TimeSpan.Zero; // reinicia desde el inicio
            media.Play();

        }

    }
}
