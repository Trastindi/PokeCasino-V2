using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta5View : Window
    {
        public Ruta5View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta5ViewModel(usuario);
        }


    }
}