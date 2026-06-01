using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta14View : Window
    {
        public Ruta14View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta14ViewModel(usuario);
        }


    }
}