using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta7View : Window
    {
        public Ruta7View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta7ViewModel(usuario);
        }


    }
}