using PK_Proyect.Commands;
using PK_Proyect.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    /// <summary>
    /// ViewModel del diálogo de cambio de Pokémon durante el combate.
    /// Usa PK_Proyect.Models.PokemonEquipoItem (la clase común del proyecto).
    /// </summary>
    public class SwitchBattlePokemonViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<PokemonEquipoItem> _equipo;
        public ObservableCollection<PokemonEquipoItem> Equipo
        {
            get => _equipo;
            set { _equipo = value; OnPropertyChanged(); }
        }

        private PokemonEquipoItem? _selectedPokemon;
        public PokemonEquipoItem? SelectedPokemon
        {
            get => _selectedPokemon;
            set
            {
                if (_selectedPokemon != null) _selectedPokemon.IsSelected = false;
                _selectedPokemon = value;
                if (_selectedPokemon != null) _selectedPokemon.IsSelected = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HaySeleccion));
            }
        }

        public bool HaySeleccion => SelectedPokemon != null;

        public RelayCommand SelectCommand { get; }

        public SwitchBattlePokemonViewModel()
        {
            _equipo = new ObservableCollection<PokemonEquipoItem>();
            SelectCommand = new RelayCommand(param =>
            {
                if (param is PokemonEquipoItem item)
                    SelectedPokemon = item;
                return Task.CompletedTask;
            });
            CargarEquipo();
        }

        private void CargarEquipo()
        {
            Equipo.Clear();
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId     = "25",
                Nombre        = "Pikachu",
                Nivel         = 5,
                TipoPrincipal = "Eléctrico",
                HpActual      = 19,
                HpMax         = 19,
                ImagenUrl     = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
                Movimientos   = new System.Collections.Generic.List<MoveModel>
                {
                    new() { Name = "Impactrueno", Pp = 30, MaxPp = 30, Type = "Eléctrico", Power = 40  },
                    new() { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",    Power = 40  },
                    new() { Name = "Cola Férrea", Pp = 15, MaxPp = 15, Type = "Acero",     Power = 100 },
                    new() { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal",    Power = 0   }
                }
            });
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId     = "4",
                Nombre        = "Charmander",
                Nivel         = 5,
                TipoPrincipal = "Fuego",
                HpActual      = 22,
                HpMax         = 22,
                ImagenUrl     = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png",
                Movimientos   = new System.Collections.Generic.List<MoveModel>
                {
                    new() { Name = "Arañazo",     Pp = 35, MaxPp = 35, Type = "Normal", Power = 40 },
                    new() { Name = "Ascuas",      Pp = 25, MaxPp = 25, Type = "Fuego",  Power = 40 },
                    new() { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal", Power = 0  },
                    new() { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",  Power = 90 }
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
