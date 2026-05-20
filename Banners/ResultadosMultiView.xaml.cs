using System.Collections.Generic;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.Views
{
    public partial class ResultadosMultiView : Window
    {
        public ResultadosMultiView(List<PokemonUser> resultados)
        {
            InitializeComponent();
            ListaResultados.ItemsSource = resultados;
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
