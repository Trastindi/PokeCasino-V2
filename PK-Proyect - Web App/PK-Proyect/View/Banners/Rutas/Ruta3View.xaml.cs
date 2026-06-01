using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta3View : Window
    {
        public Ruta3View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta3ViewModel(usuario);
        }


    }
}