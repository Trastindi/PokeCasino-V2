using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta16View : Window
    {
        public Ruta16View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta16ViewModel(usuario);
        }


    }
}