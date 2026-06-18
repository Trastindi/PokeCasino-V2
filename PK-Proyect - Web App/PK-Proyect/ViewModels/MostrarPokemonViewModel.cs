using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PK_Proyect.ViewModels
{
    public class MostrarPokemonViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Titulo { get; }
        public ObservableCollection<PokemonItemViewModel> Pokemon { get; } = new ObservableCollection<PokemonItemViewModel>();

        public MostrarPokemonViewModel(string nombreZona)
        {
            Titulo = $"Pokémon Disponibles";
        }

        public void Cargar(System.Collections.Generic.IEnumerable<PokemonItemViewModel> items)
        {
            Pokemon.Clear();
            foreach (var it in items) Pokemon.Add(it);
        }

        public class PokemonItemViewModel
        {
            public string Nombre { get; set; }
            public int Probabilidad { get; set; } // valor entero representando porcentaje
        }
    }
}
