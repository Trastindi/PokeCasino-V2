using PK_Proyect.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PK_Proyect.ViewModels
{
    public class SeleccionarPokemonViewModel : INotifyPropertyChanged
    {
        private readonly IEnumerable<PokemonUser> _todos;

        public ObservableCollection<PokemonUser> PokemonFiltrado { get; } = new();

        private PokemonUser _pokemonSeleccionado;
        public PokemonUser PokemonSeleccionado
        {
            get => _pokemonSeleccionado;
            set
            {
                _pokemonSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HaySeleccion));
            }
        }

        // El que se devuelve al cerrar la ventana con OK
        public PokemonUser PokemonElegido { get; set; }

        public bool HaySeleccion => PokemonSeleccionado != null;

        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                Filtrar();
            }
        }

        public SeleccionarPokemonViewModel(IEnumerable<PokemonUser> misPokemon)
        {
            _todos = misPokemon;
            Filtrar();
        }

        private void Filtrar()
        {
            PokemonFiltrado.Clear();
            var query = string.IsNullOrWhiteSpace(_textoBusqueda)
                ? _todos
                : _todos.Where(p => p.Nombre.ToLower().Contains(_textoBusqueda.ToLower()));
            foreach (var p in query)
                PokemonFiltrado.Add(p);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
