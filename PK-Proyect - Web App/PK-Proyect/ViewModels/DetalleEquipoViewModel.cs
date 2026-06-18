using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class DetalleEquipoViewModel
    {
        private readonly PokemonUserRepository _pokemonRepo;
        private readonly EquipoRepository _equipoRepo;

        public Equipo Equipo { get; }

        // Ahora usamos wrappers definidos en este mismo archivo (no se crea nuevo archivo)
        public ObservableCollection<PokemonWrapper> Integrantes { get; } = new ObservableCollection<PokemonWrapper>();

        public event Action<ObservableCollection<PokemonWrapper>> EquipoSeleccionado;

        public ICommand RemovePokemonCommand { get; }

        public DetalleEquipoViewModel(Equipo equipo)
        {
            Equipo = equipo ?? throw new ArgumentNullException(nameof(equipo));
            _pokemonRepo = new PokemonUserRepository();
            _equipoRepo = new EquipoRepository();

            RemovePokemonCommand = new RelayCommand(param => _ = RemovePokemonAsync(param as PokemonWrapper));

            _ = CargarIntegrantesAsync();
        }

        private async Task CargarIntegrantesAsync()
        {
            try
            {
                var todosLosPokemon = await Task.Run(() => _pokemonRepo.GetMisPokemon());

                var integrantes = todosLosPokemon
                    .Where(p => Equipo.PokemonIds.Contains(p.Id))
                    .OrderBy(p => Equipo.PokemonIds.IndexOf(p.Id))
                    .Select(p => new PokemonWrapper(p))
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Integrantes.Clear();
                    foreach (var vm in integrantes)
                        Integrantes.Add(vm);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error al cargar integrantes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning));
            }
        }

        private async Task RemovePokemonAsync(PokemonWrapper pokemonVm)
        {
            if (pokemonVm == null) return;

            var confirmar = MessageBox.Show($"¿Eliminar {pokemonVm.Nombre} del equipo {Equipo.Nombre}?",
                                            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmar != MessageBoxResult.Yes) return;

            try
            {
                var resultado = await Task.Run(() =>
                {
                    var equipos = _equipoRepo.GetMisEquipos();
                    var equipoServidor = equipos?.FirstOrDefault(e => e.Id == Equipo.Id);
                    if (equipoServidor == null)
                        return (success: false, message: "No se encontró el equipo en el servidor.");

                    if (equipoServidor.PokemonIds == null || equipoServidor.PokemonIds.Count == 0)
                        return (success: false, message: "El equipo no tiene pokémon.");

                    var index = equipoServidor.PokemonIds.IndexOf(pokemonVm.Id);
                    if (index < 0)
                        return (success: false, message: $"{pokemonVm.Nombre} no está en el equipo.");

                    equipoServidor.PokemonIds.RemoveAt(index);
                    _equipoRepo.ActualizarEquipo(equipoServidor.Id, equipoServidor.PokemonIds);

                    return (success: true, message: $"{pokemonVm.Nombre} eliminado del equipo {Equipo.Nombre}.");
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (resultado.success)
                    {
                        var toRemove = Integrantes.FirstOrDefault(p => p.Id == pokemonVm.Id);
                        if (toRemove != null)
                            Integrantes.Remove(toRemove);

                        MessageBox.Show(resultado.message, "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        public void ConfirmarEquipo()
            => EquipoSeleccionado?.Invoke(Integrantes);

        public async Task GuardarCambiosAsync()
        {
            var ids = Integrantes.Select(p => p.Id).ToList();
            await Task.Run(() => _equipoRepo.ActualizarEquipo(Equipo.Id, ids));
        }

        public async Task RefreshAsync()
        {
            await CargarIntegrantesAsync();
        }

        // Clase envoltorio definida dentro del mismo archivo (no crea nuevo archivo)
        public class PokemonWrapper
        {
            private readonly PokemonUser _model;
            public PokemonWrapper(PokemonUser model)
            {
                _model = model ?? throw new ArgumentNullException(nameof(model));
            }

            public string Id => _model.Id;
            public string Nombre => _model.Nombre;
            public int Nivel => _model.Nivel;
            public string TipoPrincipal => _model.TipoPrincipal;
            public string TipoSecundario => _model.TipoSecundario;
            public int NumeroPokedex => _model.numero_pokedex;

            // Misma lógica de URL que usas en DetallePokemonViewModel
            public string SpriteUrl => _model != null
                ? $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{_model.numero_pokedex}.png"
                : null;

            // Exponer el modelo si hace falta
            public PokemonUser Model => _model;
        }
    }
}
