using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta19View : Window
    {
        public Ruta19View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta19ViewModel(usuario);
        }


    }
}