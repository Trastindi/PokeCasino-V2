using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta9View : Window
    {
        public Ruta9View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta9ViewModel(usuario);
        }


    }
}