using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta10View : Window
    {
        public Ruta10View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta10ViewModel(usuario);
        }


    }
}