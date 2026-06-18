using PK_Proyect.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PK_Proyect.ViewModels
{
    public class ResultadoTiradasViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Titulo { get; }
        public ObservableCollection<ResultadoPokemonItem> Pokemones { get; } = new ObservableCollection<ResultadoPokemonItem>();

        public ResultadoTiradasViewModel(string nombreZona)
        {
            Titulo = $"Resultados de la tirada en {nombreZona}";
        }

        public void Cargar(System.Collections.Generic.IEnumerable<PokemonUser> items)
        {
            Pokemones.Clear();
            if (items == null) return;
            foreach (var p in items)
                Pokemones.Add(new ResultadoPokemonItem(p));
        }

        public class ResultadoPokemonItem
        {
            public string Nombre { get; }
            public string SpriteUrl { get; }

            public ResultadoPokemonItem(PokemonUser p)
            {
                Nombre = p?.Nombre ?? $"ID {p?.numero_pokedex}";
                // URL oficial de sprites (PokeAPI)
                SpriteUrl = p != null
                    ? $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{p.numero_pokedex}.png"
                    : null;
            }
        }
    }
}
