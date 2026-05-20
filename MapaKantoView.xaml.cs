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

        private void AbrirLaboratorio_Click(object sender, MouseButtonEventArgs e)
        {
            LaboratorioView ventana = new LaboratorioView(usuarioActual);
            ventana.ShowDialog();
        }


    }
}
