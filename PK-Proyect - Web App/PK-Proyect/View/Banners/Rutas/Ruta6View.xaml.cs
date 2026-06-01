using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta6View : Window
    {
        public Ruta6View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta6ViewModel(usuario);
        }


    }
}