using PK_Proyect.ViewModels.Banners.Rutas;
using System.Windows;
using PK_Proyect.Models;

namespace PK_Proyect.View.Banners.Rutas
{
    public partial class Ruta20View : Window
    {
        public Ruta20View(User usuario)
        {
            InitializeComponent();
            DataContext = new Ruta20ViewModel(usuario);
        }


    }
}