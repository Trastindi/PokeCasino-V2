using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.View;
using PK_Proyect.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class DetallePokemonViewModel
    {
        private readonly PokemonUser _pokemon;
        private readonly EquipoRepository _equipoRepo;

        public DetallePokemonViewModel(PokemonUser pokemon)
        {
            _pokemon = pokemon ?? throw new ArgumentNullException(nameof(pokemon));
            _equipoRepo = new EquipoRepository();

            AddToTeamCommand = new RelayCommand(param => _ = AddToTeamAsync(param as string));
        }

        // Propiedades para binding (adapta/añade las que necesites)
        public string Nombre => _pokemon?.Nombre;
        public int NumeroPokedex => _pokemon?.numero_pokedex ?? 0;
        public string SpriteUrl => _pokemon != null
            ? $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{_pokemon.numero_pokedex}.png"
            : null;

        // Comando público enlazable desde la vista
        public ICommand AddToTeamCommand { get; }

        /// <summary>
        /// Añade el Pokémon al equipo indicado.
        /// - Si se recibe equipoId lo usa.
        /// - Si no, abre el selector de equipos (EquipoPokemonView en modo selección).
        /// Permite duplicados; solo comprueba límite de 6.
        /// </summary>
        private async Task AddToTeamAsync(string equipoIdParam)
        {
            try
            {
                string equipoSeleccionadoId = equipoIdParam;

                // Si no se pasó equipoId, abrir selector de equipos en UI thread
                if (string.IsNullOrEmpty(equipoSeleccionadoId))
                {
                    EquipoPokemonViewModel vmEquipos = null;
                    EquipoPokemonView ventanaEquipos = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        vmEquipos = new EquipoPokemonViewModel(modoSeleccion: true);
                        ventanaEquipos = new EquipoPokemonView(vmEquipos, modoSeleccion: true)
                        {
                            Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                        };
                    });

                    // Suscribirse al evento para obtener el id seleccionado
                    void OnEquipoConfirmado(string id) => equipoSeleccionadoId = id;
                    vmEquipos.EquipoConfirmado += OnEquipoConfirmado;

                    // Mostrar modal (UI thread)
                    Application.Current.Dispatcher.Invoke(() => ventanaEquipos.ShowDialog());

                    vmEquipos.EquipoConfirmado -= OnEquipoConfirmado;
                }

                // Si sigue sin equipo seleccionado, el usuario canceló
                if (string.IsNullOrEmpty(equipoSeleccionadoId))
                    return;

                // Operaciones de I/O en background
                var resultado = await Task.Run(() =>
                {
                    var equipos = _equipoRepo.GetMisEquipos();
                    var equipo = equipos?.FirstOrDefault(t => t.Id == equipoSeleccionadoId);
                    if (equipo == null)
                        return (success: false, message: "Equipo no encontrado.");

                    if (equipo.PokemonIds == null)
                        equipo.PokemonIds = new System.Collections.Generic.List<string>();

                    // Permitir duplicados: no comprobamos si el ID ya existe.
                    if (equipo.PokemonIds.Count >= 6)
                        return (success: false, message: $"El equipo {equipo.Nombre} ya tiene 6 Pokémon. Elimina uno antes de añadir otro.");

                    equipo.PokemonIds.Add(_pokemon.Id);
                    _equipoRepo.ActualizarEquipo(equipo.Id, equipo.PokemonIds);

                    return (success: true, message: $"Se añadió {_pokemon.Nombre} al equipo {equipo.Nombre}.");
                });

                // Feedback en UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (resultado.success)
                    {
                        MessageBox.Show(resultado.message, "Añadido", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show($"Error al añadir al equipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }
    }
}
