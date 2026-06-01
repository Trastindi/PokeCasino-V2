using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta4View : Window
    {
        public Ruta4View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta4ViewModel(usuario);
        }


    }
}