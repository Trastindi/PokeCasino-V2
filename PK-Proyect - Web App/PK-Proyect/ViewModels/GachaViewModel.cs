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
            CargarZona();
        }

        // ----------------------------------------------------------------
        // Zona
        // ----------------------------------------------------------------
        private void MostrarHistorial()
        {
            var vm = new HistoricoTiradasViewModel(Usuario.Id);
            var ventana = new HistoricoTiradasView(vm);
            ventana.ShowDialog();
        }

        public void CargarZona()
        {
            PokemonDisponibles.Clear();
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);

            if (zona == null)
            {
                var todas = _zonaRepo.ObtenerTodas();
                zona = todas.FirstOrDefault(z => z.Nombre.Trim().ToLower() == NombreZona.Trim().ToLower())
                    ?? todas.FirstOrDefault(z => z.Nombre.Trim().ToLower().Contains(NombreZona.Trim().ToLower()));
            }

            if (zona == null)
            {
                MessageBox.Show($"No se encontró la zona '{NombreZona}'.",
                    "Zona no encontrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (zona.Pokemon == null || zona.Pokemon.Count == 0)
            {
                MessageBox.Show($"La zona '{zona.Nombre}' no tiene Pokémon registrados.",
                    "Zona vacía", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                if (poke == null) continue;

                PokemonDisponibles.Add(new PokemonZonaViewModel
                {
                    Id           = p.numero_pokedex,
                    Nombre       = poke.Nombre,
                    TipoPrincipal  = poke.TipoPrincipal,
                    TipoSecundario = poke.TipoSecundario,
                    Probabilidad = p.prob
                });
            }
        }

        // ----------------------------------------------------------------
        // Gacha
        // ----------------------------------------------------------------
        private PokemonZonaViewModel Tirar()
        {
            var totalProb = PokemonDisponibles.Sum(p => p.Probabilidad);
            var rnd = new Random().Next(1, totalProb + 1);
            int acumulado = 0;
            foreach (var p in PokemonDisponibles)
            {
                acumulado += p.Probabilidad;
                if (rnd <= acumulado) return p;
            }
            return null;
        }

        private void TiradaSingle()
        {
            const int COSTE = 300;
            if (Usuario.FichasCasino < COSTE)
            {
                var compra = new ComprarFichasWindow(Usuario);
                if (compra.ShowDialog() != true) return;
                Fichas = Usuario.FichasCasino;
            }

            Usuario.FichasCasino -= COSTE;
            new UserRepository().UpdateUser(Usuario);
            ActualizarFichas();

            var sorteo = Tirar();
            if (sorteo == null) return;

            var poke = _pokedexRepo.ObtenerPorId(sorteo.Id);
            if (poke == null) return;

            var resultado = _pokemonUserService.ObtenerPokemon(
                Usuario.Id, poke.numero_pokedex, poke.Nombre,
                poke.TipoPrincipal, poke.TipoSecundario,
                poke.EstadisticasBase?.Ps ?? 0);

            // Registrar historial
            new HistoricoTiradasRepository().RegistrarTirada(new HistoricoTirada
            {
                UserId       = Usuario.Id,
                PokemonId    = poke.numero_pokedex,
                NombrePokemon = poke.Nombre,
                Zona         = NombreZona,
                TipoTirada   = "single",
                Fecha        = DateTime.Now
            });

            ProcesarLevelUp(resultado);

            MessageBox.Show(
                $"¡Has obtenido a {resultado.Pokemon.Nombre}!\n" +
                $"Cantidad total: {resultado.Pokemon.Cantidad}\n" +
                $"Nivel actual: {resultado.Pokemon.Nivel}",
                "Resultado del Gacha", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TiradaMulti()
        {
            const int COSTE = 3000;
            if (Usuario.FichasCasino < COSTE)
            {
                var compra = new ComprarFichasWindow(Usuario);
                if (compra.ShowDialog() != true) return;
                Fichas = Usuario.FichasCasino;
            }

            Usuario.FichasCasino -= COSTE;
            new UserRepository().UpdateUser(Usuario);
            ActualizarFichas();

            var resultadosMulti = new List<PokemonUser>();
            var repoHist = new HistoricoTiradasRepository();

            for (int i = 0; i < 10; i++)
            {
                var sorteo = Tirar();
                if (sorteo == null) continue;

                var poke = _pokedexRepo.ObtenerPorId(sorteo.Id);
                if (poke == null) continue;

                var resultado = _pokemonUserService.ObtenerPokemon(
                    Usuario.Id, poke.numero_pokedex, poke.Nombre,
                    poke.TipoPrincipal, poke.TipoSecundario,
                    poke.EstadisticasBase?.Ps ?? 0);

                repoHist.RegistrarTirada(new HistoricoTirada
                {
                    UserId        = Usuario.Id,
                    PokemonId     = poke.numero_pokedex,
                    NombrePokemon = poke.Nombre,
                    Zona          = NombreZona,
                    TipoTirada    = "multi",
                    Fecha         = DateTime.Now
                });

                ProcesarLevelUp(resultado);
                resultadosMulti.Add(resultado.Pokemon);
            }

            new ResultadosMultiView(resultadosMulti).ShowDialog();
        }

        // ----------------------------------------------------------------
        // Procesar resultado de subida de nivel (movimiento + evolución)
        // ----------------------------------------------------------------
        private void ProcesarLevelUp(LevelUpResultado resultado)
        {
            // 1. Movimiento nuevo del nivel
            if (resultado.MovimientoAprendido != null)
            {
                if (resultado.MovimientoAprendidoDirectamente)
                {
                    MessageBox.Show(
                        $"¡{resultado.Pokemon.Nombre} ha aprendido {resultado.MovimientoAprendido}!",
                        "Nuevo movimiento", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Moveset lleno → preguntar qué olvidar
                    var ventana = new ElegirMovimientoWindow(
                        resultado.MovimientoAprendido,
                        resultado.Pokemon.MoveSet);

                    if (ventana.ShowDialog() == true)
                    {
                        _pokemonUserService.AplicarMovimiento(
                            resultado.Pokemon,
                            ventana.IndiceElegido,
                            resultado.MovimientoAprendido);
                    }
                    // Si cierra sin elegir, simplemente no aprende el movimiento
                }
            }

            // 2. Evolución
            if (resultado.Evoluciono)
            {
                MessageBox.Show(
                    $"¡Enhorabuena! ¡Tu Pokémon ha evolucionado a {resultado.NombreEvolucion}!",
                    "¡Evolución!", MessageBoxButton.OK, MessageBoxImage.Information);

                // Movimiento de la evolución
                if (resultado.MovimientoEvolucion != null)
                {
                    if (resultado.MovimientoEvolucionDirectamente)
                    {
                        MessageBox.Show(
                            $"¡{resultado.NombreEvolucion} ha aprendido {resultado.MovimientoEvolucion}!",
                            "Nuevo movimiento", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var ventana = new ElegirMovimientoWindow(
                            resultado.MovimientoEvolucion,
                            resultado.Pokemon.MoveSet);

                        if (ventana.ShowDialog() == true)
                        {
                            _pokemonUserService.AplicarMovimiento(
                                resultado.Pokemon,
                                ventana.IndiceElegido,
                                resultado.MovimientoEvolucion);
                        }
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------
        private void ActualizarFichas()
        {
            Usuario = new UserRepository().GetUserById(Usuario.Id);
            Fichas  = Usuario.FichasCasino;
        }

        private void MostrarPokemon()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);
            if (zona == null) { MessageBox.Show($"No se encontró '{NombreZona}'.", "Zona no encontrada"); return; }
            if (zona.Pokemon == null || zona.Pokemon.Count == 0) { MessageBox.Show($"Zona vacía.", "Zona vacía"); return; }

            string lista = "";
            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                lista += poke == null
                    ? $"ID {p.numero_pokedex} (No encontrado) - Prob: {p.prob}%\n"
                    : $"{poke.Nombre} - Prob: {p.prob}%\n";
            }
            MessageBox.Show(lista, $"Pokémon disponibles en {zona.Nombre}");
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
            MessageBox.Show($"Nombre: {zona.Nombre}\nRegión: {zona.Region}\nTipo: {zona.Tipo}\nPokémon: {zona.Pokemon?.Count}",
                "Zona encontrada", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DebugMostrarIdYNombre()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);
            if (zona == null) { MessageBox.Show("Zona no encontrada."); return; }
            if (zona.Pokemon == null || zona.Pokemon.Count == 0) { MessageBox.Show("Sin Pokémon."); return; }
            string lista = "";
            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                lista += poke == null
                    ? $"ID {p.numero_pokedex} → NO ENCONTRADO\n"
                    : $"ID {p.numero_pokedex} → {poke.Nombre} (real: {poke.numero_pokedex})\n";
            }
            MessageBox.Show(lista, "Relación Zona → Pokédex");
        }

        public void DebugMostrarPokedex()
        {
            var todos = _pokedexRepo.ObtenerTodos();
            if (todos == null || todos.Count == 0) { MessageBox.Show("Pokédex vacía."); return; }
            MessageBox.Show(string.Join("\n", todos.Select(p => $"ID {p.numero_pokedex} → {p.Nombre}")), "Pokédex completa");
        }
    }
}
