using PK_Proyect.Models;
using PK_Proyect.ViewModels.Banners;
using PK_Proyect.ViewModels.Banners.Lugares;
using System.Windows;
namespace PK_Proyect.View.Banners.Lugares
{
    /// <summary>

    /// </summary>
    public partial class MonteMoonView : Window
    {
        public MonteMoonView(User usuario)
        {
            InitializeComponent();
            DataContext = new MonteMoonViewModel(usuario);
        }
    }
}

