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
    /// Carga el equipo del jugador (desde sesión / servicio) y permite seleccionar uno.
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

        private PokemonEquipoItem _selectedPokemon;
        public PokemonEquipoItem SelectedPokemon
        {
            get => _selectedPokemon;
            set
            {
                // Deseleccionar el anterior
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
            Equipo = new ObservableCollection<PokemonEquipoItem>();
            SelectCommand = new RelayCommand(param =>
            {
                if (param is PokemonEquipoItem item)
                    SelectedPokemon = item;
                return Task.CompletedTask;
            });
            CargarEquipo();
        }

        /// <summary>
        /// Carga el equipo. Sustituir por llamada al servicio/sesión cuando esté disponible.
        /// Actualmente usa datos de placeholder.
        /// </summary>
        private void CargarEquipo()
        {
            Equipo.Clear();

            // TODO: reemplazar con datos reales de la sesión del jugador
            Equipo.Add(new PokemonEquipoItem
            {
                Nombre       = "Pikachu",
                Nivel        = 5,
                TipoPrincipal = "Eléctrico",
                HpActual     = 19,
                HpMax        = 19,
                ImagenUrl    = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
                Movimientos  = new System.Collections.Generic.List<MoveModel>
                {
                    new MoveModel { Name = "Impactrueno", Pp = 30, MaxPp = 30, Type = "Eléctrico", Power = 40 },
                    new MoveModel { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",    Power = 40 },
                    new MoveModel { Name = "Cola Férrea", Pp = 15, MaxPp = 15, Type = "Acero",     Power = 100 },
                    new MoveModel { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal",    Power = 0 }
                }
            });
            Equipo.Add(new PokemonEquipoItem
            {
                Nombre       = "Charmander",
                Nivel        = 5,
                TipoPrincipal = "Fuego",
                HpActual     = 22,
                HpMax        = 22,
                ImagenUrl    = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png",
                Movimientos  = new System.Collections.Generic.List<MoveModel>
                {
                    new MoveModel { Name = "Arañazo",     Pp = 35, MaxPp = 35, Type = "Normal", Power = 40 },
                    new MoveModel { Name = "Ascuas",      Pp = 25, MaxPp = 25, Type = "Fuego",  Power = 40 },
                    new MoveModel { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal", Power = 0 },
                    new MoveModel { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",  Power = 90 }
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>Representa un Pokémon del equipo en el diálogo de cambio.</summary>
    public class PokemonEquipoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Nombre        { get; set; }
        public int    Nivel         { get; set; }
        public string TipoPrincipal { get; set; }
        public int    HpActual      { get; set; }
        public int    HpMax         { get; set; }
        public string ImagenUrl     { get; set; }
        public System.Collections.Generic.List<MoveModel> Movimientos { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }
    }
}
