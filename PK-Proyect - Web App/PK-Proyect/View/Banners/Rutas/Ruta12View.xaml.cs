using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta12View : Window
    {
        public Ruta12View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta12ViewModel(usuario);
        }


    }
}