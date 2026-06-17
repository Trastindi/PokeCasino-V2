using System.Windows;
using PK_Proyect.ViewModels;

namespace PK_Proyect.View
{
    public partial class EquipoPokemonView : Window
    {
        // Constructor normal: gestion de equipo desde el menu
        public EquipoPokemonView(EquipoPokemonViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        // Constructor modo seleccion: elegir equipo antes de la batalla
        public EquipoPokemonView(EquipoPokemonViewModel vm, bool modoSeleccion)
        {
            InitializeComponent();
            DataContext = vm;

            if (modoSeleccion)
            {
                // Adaptar titulo para dejar claro al usuario que debe elegir su equipo
                Title = "Elige tu equipo para la batalla";

                // Cerrar la ventana automaticamente si el VM confirma o cancela
                vm.EquipoConfirmado  += _ => this.Close();
                vm.SeleccionCancelada += () => this.Close();
            }
        }
    }
}
