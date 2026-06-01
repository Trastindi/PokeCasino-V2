using PK_Proyect.Models;
using PK_Proyect.ViewModels.Banners;
using PK_Proyect.ViewModels.Banners.Lugares;
using System.Windows;
namespace PK_Proyect.View.Banners.Lugares
{
 
    public partial class TorrePokemonView : Window
    {
        public TorrePokemonView(User usuario)
        {
            InitializeComponent();
            DataContext = new TorrePokemonViewModel(usuario);
        }
    }
}
