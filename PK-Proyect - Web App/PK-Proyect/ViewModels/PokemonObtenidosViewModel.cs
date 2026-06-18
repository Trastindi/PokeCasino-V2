using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PK_Proyect.ViewModels
{
    public class PokemonObtenidosViewModel : INotifyPropertyChanged
    {
        private readonly PokemonUserRepository _repo;
        private readonly string _userId;

        // Lista original (nunca se modifica tras la carga)
        private readonly ObservableCollection<PokemonUser> _todos = new();

        // Vista filtrada que se bindea en el XAML
        public ICollectionView ListaFiltrada { get; }

        // ── Buscador ────────────────────────────────────────────────────────
        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (_textoBusqueda == value) return;
                _textoBusqueda = value;
                OnPropertyChanged();
                ListaFiltrada.Refresh();
            }
        }

        // ── Constructor ─────────────────────────────────────────────────────
        public PokemonObtenidosViewModel(string userId)
        {
            _repo   = new PokemonUserRepository();
            _userId = userId;

            ListaFiltrada = CollectionViewSource.GetDefaultView(_todos);
            ListaFiltrada.Filter = FiltrarPokemon;

            _ = CargarAsync();
        }

        // ── Filtro ──────────────────────────────────────────────────────────
        private bool FiltrarPokemon(object item)
        {
            if (string.IsNullOrWhiteSpace(_textoBusqueda)) return true;
            return item is PokemonUser p &&
                   p.Nombre != null &&
                   p.Nombre.Contains(_textoBusqueda.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }

        // ── Carga ───────────────────────────────────────────────────────────
        private async Task CargarAsync()
        {
            var pokes = await Task.Run(() =>
                _repo.GetPokemonsByUser(_userId)
                     .OrderByDescending(p => p.FechaObtenido)
                     .ToList()
            );

            Application.Current.Dispatcher.Invoke(() =>
            {
                _todos.Clear();
                foreach (var p in pokes)
                    _todos.Add(p);
                ListaFiltrada.Refresh();
            });
        }

        // ── INotifyPropertyChanged ───────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
