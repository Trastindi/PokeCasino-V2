using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta25View : Window
    {
        public Ruta25View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta25ViewModel(usuario);
        }


    }
}