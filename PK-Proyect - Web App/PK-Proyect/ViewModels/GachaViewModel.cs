using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Services;
using PK_Proyect.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using System.Text;

namespace PK_Proyect.ViewModels.Banners
{
    public class GachaViewModel : INotifyPropertyChanged
    {
        protected readonly ZonaRepository _zonaRepo;
        protected readonly PokedexRepository _pokedexRepo;
        private readonly PokemonUserService _pokemonUserService;

        // Singleton para evitar misma semilla al tirar x10 en rapida sucesion
        private static readonly Random _rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User Usuario { get; set; }
        public string NombreZona { get; set; }
        public string ZonaNombre => NombreZona;

        public ObservableCollection<PokemonZonaViewModel> PokemonDisponibles { get; set; }

        public ICommand Tirar1Command         { get; }
        public ICommand Tirar10Command        { get; }
        public ICommand MostrarPokemonCommand  { get; }
        public ICommand MostrarZonasCommand    { get; }
        public ICommand HistorialCommand       { get; }
        public ICommand DebugBuscarZonaCommand { get; }
        public ICommand DebugIdNombreCommand   { get; }
        public ICommand DebugPokedexCommand    { get; }

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
            Usuario    = usuario;
            NombreZona = nombreZona;

            _zonaRepo    = new ZonaRepository();
            _pokedexRepo = new PokedexRepository();
            PokemonDisponibles = new ObservableCollection<PokemonZonaViewModel>();

            Tirar1Command  = new AsyncRelayCommand(async () => await TiradaSingleAsync());
            Tirar10Command = new AsyncRelayCommand(async () => await TiradaMultiAsync());

            // MostrarPokemonCommand = new RelayCommand(_ => MostrarPokemon());
            MostrarPokemonCommand = new AsyncRelayCommand(async() => await MostrarPokemonAsync());
            MostrarZonasCommand   = new RelayCommand(_ => MostrarZonasBD());
            HistorialCommand      = new AsyncRelayCommand(async () => await MostrarHistorialAsync());
            _pokemonUserService   = new PokemonUserService();
            Fichas = Usuario.FichasCasino;

            _ = CargarZonaAsync();
        }

        private async Task MostrarHistorialAsync()
        {
            var vm = new HistoricoTiradasViewModel(Usuario.Id);
            await vm.CargarAsync();
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

                    var mapa = new Dictionary<int, Pokemon>();
                    foreach (PokemonZona p in z.Pokemon)
                    {
                        Pokemon poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                        if (poke != null)
                            mapa[p.numero_pokedex] = poke;
                    }
                    pokesEncontrados = mapa;
                });

                if (zonaEncontrada == null)
                {
                    MessageBox.Show($"No se encontr\u00f3 la zona '{NombreZona}'.",
                        "Zona no encontrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (zonaEncontrada.Pokemon == null || zonaEncontrada.Pokemon.Count == 0)
                {
                    MessageBox.Show($"La zona '{zonaEncontrada.Nombre}' no tiene Pok\u00e9mon registrados.",
                        "Zona vac\u00eda", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (PokemonZona p in zonaEncontrada.Pokemon)
                {
                    if (pokesEncontrados == null || !pokesEncontrados.TryGetValue(p.numero_pokedex, out Pokemon poke))
                        continue;

                    PokemonDisponibles.Add(new PokemonZonaViewModel
                    {
                        Id             = p.numero_pokedex,
                        Nombre         = poke.Nombre,
                        TipoPrincipal  = poke.TipoPrincipal,
                        TipoSecundario = poke.TipoSecundario,
                        Probabilidad   = p.prob
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CargarZonaAsync] Excepci\u00f3n: {ex}");
                MessageBox.Show(
                    $"Error al cargar la zona: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Cargando = false;
            }
        }

        public void CargarZona() => _ = CargarZonaAsync();

        // ----------------------------------------------------------------
        // Gacha
        // ----------------------------------------------------------------
        private PokemonZonaViewModel Tirar()
        {
            if (PokemonDisponibles == null || PokemonDisponibles.Count == 0)
                return null;

            var totalProb = PokemonDisponibles.Sum(p => p.Probabilidad);
            if (totalProb <= 0) return null;

            var rnd = _rnd.Next(1, totalProb + 1);
            int acumulado = 0;
            foreach (var p in PokemonDisponibles)
            {
                acumulado += p.Probabilidad;
                if (rnd <= acumulado) return p;
            }
            return null;
        }

        private async Task TiradaSingleAsync()
        {
            const int COSTE = 300;
            try
            {
                if (Usuario.FichasCasino < COSTE)
                {
                    var compra = new ComprarFichasWindow(Usuario);
                    if (compra.ShowDialog() != true) return;
                    Fichas = Usuario.FichasCasino;
                }

                // Parte sincrona CPU-bound
                var sorteo = await Task.Run(() =>
                {
                    Usuario.FichasCasino -= COSTE;
                    new UserRepository().UpdateUser(Usuario);
                    Debug.WriteLine($"[TIRADA SINGLE] Fichas despu\u00e9s de cobro: {Usuario.FichasCasino}");
                    return Tirar();
                });

                if (sorteo == null)
                {
                    MessageBox.Show("No se pudo realizar el sorteo. La zona no tiene Pok\u00e9mon disponibles.",
                        "Sorteo fallido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var poke = _pokedexRepo.ObtenerPorId(sorteo.Id);
                if (poke == null)
                {
                    MessageBox.Show($"No se encontr\u00f3 el Pok\u00e9mon con ID {sorteo.Id} en la Pok\u00e9dex.",
                        "Error de datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parte asincrona fuera de Task.Run para evitar Task<Task> no unwrapped
                var pokemon = await _pokemonUserService.ObtenerPokemonAsync(
                    poke.numero_pokedex, poke.Nombre,
                    poke.TipoPrincipal, poke.TipoSecundario,
                    poke.EstadisticasBase?.Ps ?? 0);

                await Task.Run(() => new HistoricoTiradasRepository().RegistrarTirada(new HistoricoTirada
                {
                    UserId        = Usuario.Id,
                    PokemonId     = poke.numero_pokedex,
                    NombrePokemon = poke.Nombre,
                    Zona          = NombreZona,
                    TipoTirada    = "single",
                    Fecha         = DateTime.Now
                }));

                await ActualizarFichasAsync();

                if (pokemon != null)
                    MessageBox.Show(
                        $"\u00a1Has obtenido a {pokemon.Nombre}!",
                        "Resultado del Gacha", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(
                        $"Se registr\u00f3 la tirada pero hubo un problema al guardar {poke.Nombre}. Intenta de nuevo.",
                        "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TiradaSingleAsync] Excepci\u00f3n: {ex}");
                MessageBox.Show(
                    $"Error al realizar la tirada:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TiradaMultiAsync()
        {
            const int COSTE = 3000;
            try
            {
                if (Usuario.FichasCasino < COSTE)
                {
                    var compra = new ComprarFichasWindow(Usuario);
                    if (compra.ShowDialog() != true) return;
                    Fichas = Usuario.FichasCasino;
                }

                // Parte sincrona CPU-bound
                List<(Pokemon poke, PokemonZonaViewModel sorteo)> sorteos = null;
                sorteos = await Task.Run(() =>
                {
                    Usuario.FichasCasino -= COSTE;
                    new UserRepository().UpdateUser(Usuario);
                    Debug.WriteLine($"[TIRADA MULTI] Fichas despu\u00e9s de cobro: {Usuario.FichasCasino}");

                    var lista = new List<(Pokemon, PokemonZonaViewModel)>();
                    for (int i = 0; i < 10; i++)
                    {
                        var s = Tirar();
                        if (s == null) continue;
                        var p = _pokedexRepo.ObtenerPorId(s.Id);
                        if (p != null) lista.Add((p, s));
                    }
                    return lista;
                });

                // Parte asincrona fuera de Task.Run para evitar Task<Task> no unwrapped
                var pokemonesObtenidos = new List<PokemonUser>();
                var repoHist = new HistoricoTiradasRepository();

                foreach (var (poke, _) in sorteos)
                {
                    var obtenido = await _pokemonUserService.ObtenerPokemonAsync(
                        poke.numero_pokedex, poke.Nombre,
                        poke.TipoPrincipal, poke.TipoSecundario,
                        poke.EstadisticasBase?.Ps ?? 0);

                    await Task.Run(() => repoHist.RegistrarTirada(new HistoricoTirada
                    {
                        UserId        = Usuario.Id,
                        PokemonId     = poke.numero_pokedex,
                        NombrePokemon = poke.Nombre,
                        Zona          = NombreZona,
                        TipoTirada    = "multi",
                        Fecha         = DateTime.Now
                    }));

                    if (obtenido != null)
                        pokemonesObtenidos.Add(obtenido);
                }

                await ActualizarFichasAsync();
                new ResultadosMultiView(pokemonesObtenidos).ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TiradaMultiAsync] Excepci\u00f3n: {ex}");
                MessageBox.Show(
                    $"Error al realizar la tirada m\u00faltiple:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private async Task ActualizarFichasAsync()
        {
            try
            {
                Usuario = await ApiClient.GetAsync<User>($"/usuarios/{Usuario.Id}");
                Fichas  = Usuario.FichasCasino;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ActualizarFichasAsync] Error al refrescar fichas: {ex.Message}");
                // No cerramos la app: simplemente actualizamos con el valor local
                Fichas = Usuario.FichasCasino;
                MessageBox.Show(
                    "No se pudieron refrescar las fichas desde el servidor.\nSe muestra el saldo local.",
                    "Aviso de conexi\u00f3n", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // private void MostrarPokemon()
        // {
        //     var zona = _zonaRepo.ObtenerPorNombre(NombreZona);
        //     if (zona == null) { MessageBox.Show($"No se encontr\u00f3 '{NombreZona}'.", "Zona no encontrada"); return; }
        //     if (zona.Pokemon == null || zona.Pokemon.Count == 0) { MessageBox.Show($"Zona vac\u00eda.", "Zona vac\u00eda"); return; }

        //     string lista = "";
        //     foreach (var p in zona.Pokemon)
        //     {
        //         var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
        //         lista += poke == null
        //             ? $"ID {p.numero_pokedex} (No encontrado) - Prob: {p.prob}%\n"
        //             : $"{poke.Nombre} - Prob: {p.prob}%\n";
        //     }
        //     MessageBox.Show(lista, $"Pok\u00e9mon disponibles en {zona.Nombre}");
        // }


        private static string NormalizarNombres(string s)
{
    if (string.IsNullOrWhiteSpace(s)) return string.Empty;
    // Trim, colapsar espacios múltiples, pasar a minúsculas
    var trimmed = System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
    // Normalizar y quitar diacríticos (tildes)
    var normalized = trimmed.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();
    foreach (var ch in normalized)
    {
        var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
        if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
    }
    return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
}

private async Task MostrarPokemonAsync()
{
    try
    {
        Debug.WriteLine($"[MostrarPokemonAsync] Inicio. NombreZona='{NombreZona}'");

        var zona = await Task.Run(() =>
        {
            // Intento rápido por nombre exacto en repo
            var z = _zonaRepo.ObtenerPorNombre(NombreZona);
            if (z != null) return z;

            // Si no hay coincidencia exacta, buscar en todas normalizando
            var todas = _zonaRepo.ObtenerTodas();
            if (todas == null) return null;

            var buscada = NormalizarNombres(NombreZona);

            // Búsqueda exacta normalizada
            z = todas.FirstOrDefault(x => NormalizarNombres(x?.Nombre) == buscada);
            if (z != null) return z;

   
            z = todas.FirstOrDefault(x => NormalizarNombres(x?.Nombre).Contains(buscada));
            return z;
        });

        Debug.WriteLine($"[MostrarPokemonAsync] Zona encontrada: '{zona?.Nombre ?? "null"}'");

        if (zona == null)
        {
            MessageBox.Show($"No se encontró '{NombreZona}'.", "Zona no encontrada");
            return;
        }

        if (zona.Pokemon == null || zona.Pokemon.Count == 0)
        {
            MessageBox.Show($"Zona vacía.", "Zona vacía");
            return;
        }

        var lines = await Task.Run(() =>
        {
            var lista = new List<string>();
            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                lista.Add(poke == null
                    ? $"ID {p.numero_pokedex} (No encontrado) - Prob: {p.prob}%"
                    : $"{poke.Nombre} - Prob: {p.prob}%");
            }
            return lista;
        });

        MessageBox.Show(string.Join("\n", lines), $"Pokémon disponibles en {zona.Nombre}");
        Debug.WriteLine("[MostrarPokemonAsync] Fin OK");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[MostrarPokemonAsync] Excepción: {ex}");
        MessageBox.Show($"Error al mostrar los Pokémon de la zona:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}


        

        public void MostrarZonasBD()
        {
            var zonas = _zonaRepo.ObtenerTodas();
            if (zonas == null || zonas.Count == 0)
            { MessageBox.Show("No hay zonas.", "Zonas", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            MessageBox.Show(string.Join("\n", zonas.Select(z => z.Nombre)),
                "Zonas disponibles", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DebugBuscarZona()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);
            if (zona == null)
            { MessageBox.Show($"No encontrada: \"{NombreZona}\"", "Zona no encontrada", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            MessageBox.Show($"Nombre: {zona.Nombre}\nRegi\u00f3n: {zona.Region}\nTipo: {zona.Tipo}\nPok\u00e9mon: {zona.Pokemon?.Count}",
                "Zona encontrada", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DebugMostrarIdYNombre()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);
            if (zona == null) { MessageBox.Show("Zona no encontrada."); return; }
            if (zona.Pokemon == null || zona.Pokemon.Count == 0) { MessageBox.Show("Sin Pok\u00e9mon."); return; }
            string lista = "";
            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                lista += poke == null
                    ? $"ID {p.numero_pokedex} \u2192 NO ENCONTRADO\n"
                    : $"ID {p.numero_pokedex} \u2192 {poke.Nombre} (real: {poke.numero_pokedex})\n";
            }
            MessageBox.Show(lista, "Relaci\u00f3n Zona \u2192 Pok\u00e9dex");
        }

        public void DebugMostrarPokedex()
        {
            var todos = _pokedexRepo.ObtenerTodos();
            if (todos == null || todos.Count == 0) { MessageBox.Show("Pok\u00e9dex vac\u00eda."); return; }
            MessageBox.Show(string.Join("\n", todos.Select(p => $"ID {p.numero_pokedex} \u2192 {p.Nombre}")), "Pok\u00e9dex completa");
        }
    }
}
