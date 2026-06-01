using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta22View : Window
    {
        public Ruta22View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta22ViewModel(usuario);
        }


    }
}