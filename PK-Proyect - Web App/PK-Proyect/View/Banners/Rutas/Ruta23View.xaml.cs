using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta23View : Window
    {
        public Ruta23View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta23ViewModel(usuario);
        }


    }
}