using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels.Banners
{
    public class GachaViewModel : INotifyPropertyChanged
    {
        protected readonly ZonaRepository _zonaRepo;
        protected readonly PokedexRepository _pokedexRepo;
        private readonly PokemonUserService _pokemonUserService;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User Usuario { get; set; }
        public string NombreZona { get; set; }
        public ObservableCollection<PokemonZonaViewModel> PokemonDisponibles { get; set; }

        public ICommand Tirar1Command   { get; }
        public ICommand Tirar10Command  { get; }
        public ICommand MostrarPokemonCommand { get; }
        public ICommand MostrarZonasCommand   { get; }
        public ICommand HistorialCommand      { get; }
        public ICommand DebugBuscarZonaCommand  { get; }
        public ICommand DebugIdNombreCommand    { get; }
        public ICommand DebugPokedexCommand     { get; }

        private int _fichas;
        public int Fichas
        {
            get => _fichas;
            set { _fichas = value; OnPropertyChanged(nameof(Fichas)); }
        }

        private bool _cargando;
        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(nameof(Cargando)); }
        }

        public GachaViewModel(User usuario, string nombreZona)
        {
            Usuario   = usuario;
            NombreZona = nombreZona;

            _zonaRepo    = new ZonaRepository();
            _pokedexRepo = new PokedexRepository();
            PokemonDisponibles = new ObservableCollection<PokemonZonaViewModel>();

            Tirar1Command         = new RelayCommand(_ => TiradaSingle());
            Tirar10Command        = new RelayCommand(_ => TiradaMulti());
            MostrarPokemonCommand = new RelayCommand(_ => MostrarPokemon());
            MostrarZonasCommand   = new RelayCommand(_ => MostrarZonasBD());
            HistorialCommand      = new RelayCommand(_ => MostrarHistorial());
            _pokemonUserService = new PokemonUserService();
            Fichas = Usuario.FichasCasino;

            _ = CargarZonaAsync();
        }

        private void MostrarHistorial()
        {
            var vm = new HistoricoTiradasViewModel(Usuario.Id);
            var ventana = new HistoricoTiradasView(vm);
            ventana.ShowDialog();
        }

        public async Task CargarZonaAsync()
        {
            Cargando = true;
            PokemonDisponibles.Clear();

            try
            {
                Zona zonaEncontrada = null;
                Dictionary<int, Pokemon> pokesEncontrados = null;

                await Task.Run(() =>
                {
                    Zona z = _zonaRepo.ObtenerPorNombre(NombreZona);

                    if (z == null)
                    {
                        List<Zona> todas = _zonaRepo.ObtenerTodas();
                        z = todas.FirstOrDefault(x => x.Nombre.Trim().ToLower() == NombreZona.Trim().ToLower())
                         ?? todas.FirstOrDefault(x => x.Nombre.Trim().ToLower().Contains(NombreZona.Trim().ToLower()));
                    }

                    if (z == null || z.Pokemon == null || z.Pokemon.Count == 0)
                    {
                        zonaEncontrada = z;
                        return;
                    }

                    zonaEncontrada = z;
                    pokesEncontrados = new Dictionary<int, Pokemon>();

                    foreach (var p in z.Pokemon)
                    {
                        var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                        if (poke != null)
                            pokesEncontrados[p.numero_pokedex] = poke;
                    }
                });

                if (zonaEncontrada == null)
                {
                    MessageBox.Show($"No se encontró la zona '{NombreZona}' en Mongo.");
                    return;
                }

                if (zonaEncontrada.Pokemon == null || zonaEncontrada.Pokemon.Count == 0)
                {
                    MessageBox.Show($"La zona '{NombreZona}' no tiene Pokémon configurados.");
                    return;
                }

                foreach (var p in zonaEncontrada.Pokemon)
                {
                    if (pokesEncontrados != null && pokesEncontrados.TryGetValue(p.numero_pokedex, out var poke))
                    {
                        PokemonDisponibles.Add(new PokemonZonaViewModel
                        {
                            NumeroPokedex = p.numero_pokedex,
                            Nombre = poke.Nombre,
                            Rareza = p.rareza
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando la zona: " + ex.Message);
            }
            finally
            {
                Cargando = false;
            }
        }

        private async void TiradaSingle()
        {
            await EjecutarTiradaAsync(1);
        }

        private async void TiradaMulti()
        {
            await EjecutarTiradaAsync(10);
        }

        private async Task EjecutarTiradaAsync(int cantidad)
        {
            if (Cargando) return;

            int coste = cantidad == 1 ? 1 : 10;
            if (Fichas < coste)
            {
                MessageBox.Show("No tienes suficientes fichas.");
                return;
            }

            try
            {
                Cargando = true;
                var resultados = new List<LevelUpResultado>();

                await Task.Run(() =>
                {
                    var random = new Random();
                    for (int i = 0; i < cantidad; i++)
                    {
                        var elegido = ElegirPokemonAleatorio(random);
                        if (elegido == null) continue;

                        var poke = _pokedexRepo.ObtenerPorId(elegido.NumeroPokedex);
                        if (poke == null) continue;

                        var resultado = _pokemonUserService.ObtenerPokemon(
                            Usuario.Id,
                            poke.Id,
                            poke.Nombre,
                            poke.Tipo1,
                            poke.Tipo2,
                            poke.CurrentHp
                        );

                        if (resultado != null)
                            resultados.Add(resultado);
                    }
                });

                Fichas -= coste;
                Usuario.FichasCasino = Fichas;

                if (resultados.Count == 0)
                {
                    MessageBox.Show("No se pudo obtener ningún Pokémon.");
                    return;
                }

                string resumen = string.Join(Environment.NewLine, resultados
                    .Where(r => r.Pokemon != null)
                    .Select(r => $"- {r.Pokemon.Nombre} Nv.{r.Pokemon.Nivel}"));

                MessageBox.Show($"Has obtenido:{Environment.NewLine}{resumen}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error durante la tirada: " + ex.Message);
            }
            finally
            {
                Cargando = false;
            }
        }

        private PokemonZonaViewModel ElegirPokemonAleatorio(Random random)
        {
            if (PokemonDisponibles == null || PokemonDisponibles.Count == 0)
                return null;

            var pesos = PokemonDisponibles.Select(p => Math.Max(1, 101 - p.Rareza)).ToList();
            int total = pesos.Sum();
            int tirada = random.Next(1, total + 1);
            int acumulado = 0;

            for (int i = 0; i < PokemonDisponibles.Count; i++)
            {
                acumulado += pesos[i];
                if (tirada <= acumulado)
                    return PokemonDisponibles[i];
            }

            return PokemonDisponibles.LastOrDefault();
        }

        private void MostrarPokemon()
        {
            MessageBox.Show(string.Join(Environment.NewLine,
                PokemonDisponibles.Select(p => $"#{p.NumeroPokedex} {p.Nombre} (Rareza {p.Rareza})")));
        }

        private void MostrarZonasBD()
        {
            try
            {
                var zonas = _zonaRepo.ObtenerTodas();
                MessageBox.Show(string.Join(Environment.NewLine, zonas.Select(z => z.Nombre)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error mostrando zonas: " + ex.Message);
            }
        }
    }
}
