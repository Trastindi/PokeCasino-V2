using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;

using PK_Proyect.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using PK_Proyect.Views;

using System.Windows.Input;
using System.Windows;



namespace PK_Proyect.ViewModels.Banners
{
    public class GachaViewModel : INotifyPropertyChanged
    {
        protected readonly ZonaRepository _zonaRepo;
        protected readonly PokedexRepository _pokedexRepo;
        private readonly PokemonUserService _pokemonUserService;


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public User Usuario { get; set; }
        public string NombreZona { get; set; }

        public ObservableCollection<PokemonZonaViewModel> PokemonDisponibles { get; set; }

        public ICommand Tirar1Command { get; }
        public ICommand Tirar10Command { get; }
        public ICommand MostrarPokemonCommand { get; }
        public ICommand MostrarZonasCommand { get; }
        public ICommand DebugBuscarZonaCommand { get; }

        public ICommand DebugIdNombreCommand { get; }


        public ICommand DebugPokedexCommand { get; }


        public ICommand HistorialCommand { get; }



        private int _fichas;
        public int Fichas
        {
            get => _fichas;
            set
            {
                _fichas = value;
                OnPropertyChanged(nameof(Fichas));
            }
        }





        public GachaViewModel(User usuario, string nombreZona)
        {
            Usuario = usuario;
            NombreZona = nombreZona;

            _zonaRepo = new ZonaRepository();
            _pokedexRepo = new PokedexRepository();

            PokemonDisponibles = new ObservableCollection<PokemonZonaViewModel>();

            Tirar1Command = new RelayCommand(TiradaSingle);
            Tirar10Command = new RelayCommand(TiradaMulti);
            MostrarPokemonCommand = new RelayCommand(MostrarPokemon);
            MostrarZonasCommand = new RelayCommand(MostrarZonasBD);
            //DebugBuscarZonaCommand = new RelayCommand(DebugBuscarZona);
            //DebugIdNombreCommand = new RelayCommand(DebugMostrarIdYNombre); 
            //DebugPokedexCommand = new RelayCommand(DebugMostrarPokedex);

            HistorialCommand = new RelayCommand(MostrarHistorial);


            _pokemonUserService = new PokemonUserService();


            Fichas = Usuario.FichasCasino;


            CargarZona();
        }



        private void MostrarHistorial()
        {
            var vm = new HistoricoTiradasViewModel(Usuario.Id);
            var ventana = new HistoricoTiradasView(vm);
            ventana.ShowDialog();
        }


        public void CargarZona()
        {   
         

            PokemonDisponibles.Clear();

            // Intento normal
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);


            //System.Windows.MessageBox.Show("Tipo real de zona.Pokemon[0]: " + zona.Pokemon[0].GetType().FullName);
            // Si no la encuentra, buscamos por coincidencia parcial
            if (zona == null)
            {
                var todas = _zonaRepo.ObtenerTodas();

                zona = todas.FirstOrDefault(z =>
                    z.Nombre.Trim().ToLower() == NombreZona.Trim().ToLower());

                // Si aún así no la encuentra, probamos coincidencia flexible
                if (zona == null)
                {
                    zona = todas.FirstOrDefault(z =>
                        z.Nombre.Trim().ToLower().Contains(NombreZona.Trim().ToLower()));
                }
            }

            // Si después de todo sigue siendo null, salimos
            if (zona == null)
            {
                System.Windows.MessageBox.Show(
                    $"No se encontró la zona '{NombreZona}'.\n\n" +
                    "Revisa si el nombre coincide exactamente con el de la base de datos.",
                    "Zona no encontrada",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            // Si la zona existe pero no tiene Pokémon
            if (zona.Pokemon == null || zona.Pokemon.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    $"La zona '{zona.Nombre}' no tiene Pokémon registrados.",
                    "Zona vacía",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
                return;
            }

            // Cargar Pokémon
            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);
                if (poke == null)
                    continue;

                PokemonDisponibles.Add(new PokemonZonaViewModel
                {
                    Id = p.numero_pokedex,
                    Nombre = poke.Nombre,
                    TipoPrincipal = poke.TipoPrincipal,
                    TipoSecundario = poke.TipoSecundario,
                    Probabilidad = p.prob
                });
            }
        }


        // ------------------------------
        // SISTEMA DE GACHA UNIVERSAL
        // ------------------------------

        private PokemonZonaViewModel Tirar()
        {
            var totalProb = PokemonDisponibles.Sum(p => p.Probabilidad);
            var rnd = new Random().Next(1, totalProb + 1);

            int acumulado = 0;

            foreach (var p in PokemonDisponibles)
            {
                acumulado += p.Probabilidad;
                if (rnd <= acumulado)
                    return p;
            }

            return null;
        }

        private void TiradaSingle()
        {
            const int COSTE_Single = 300;

            
            if (Usuario.FichasCasino < COSTE_Single)
            {
                var ventana = new ComprarFichasWindow(Usuario);
                bool? comprado = ventana.ShowDialog();

                if (comprado != true)
                    return;

              
                Fichas = Usuario.FichasCasino;
            }

            
            Usuario.FichasCasino -= COSTE_Single;

            var userRepo = new UserRepository();
            userRepo.UpdateUser(Usuario);


            ActualizarFichas();


            var resultadoZona = Tirar();
            if (resultadoZona == null)
                return;

            
            var poke = _pokedexRepo.ObtenerPorId(resultadoZona.Id);

            var resultado = _pokemonUserService.ObtenerPokemon(
                Usuario.Id,
                poke.numero_pokedex,
                poke.Nombre,
                poke.TipoPrincipal,
                poke.TipoSecundario,
                poke.EstadisticasBase[0]
            );

            var repoHist = new HistoricoTiradasRepository();
            repoHist.RegistrarTirada(new HistoricoTirada
            {
                UserId = Usuario.Id,
                PokemonId = poke.numero_pokedex,
                NombrePokemon = poke.Nombre,
                Zona = NombreZona,
                TipoTirada = "single",
                Fecha = DateTime.Now
            });

            
            MessageBox.Show(
                $"¡Has obtenido a {poke.Nombre}!\n" +
                $"Cantidad total: {resultado.Cantidad}\n" +
                $"Nivel actual: {resultado.Nivel}",
                "Resultado del Gacha",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }



        private void TiradaMulti()
        {
            const int COSTE_MULTI = 3000;

           
            if (Usuario.FichasCasino < COSTE_MULTI)
            {
                var ventana = new ComprarFichasWindow(Usuario);
                bool? comprado = ventana.ShowDialog();

                if (comprado != true)
                    return;

              

                Fichas = Usuario.FichasCasino;
            }

            
            Usuario.FichasCasino -= COSTE_MULTI;

            var userRepo = new UserRepository();
            userRepo.UpdateUser(Usuario);


            ActualizarFichas();


            List<PokemonUser> resultadosMulti = new List<PokemonUser>();
            var repoHist = new HistoricoTiradasRepository();

            
            for (int i = 0; i < 10; i++)
            {
                var resultadoZona = Tirar();
                if (resultadoZona == null)
                    continue;

                var poke = _pokedexRepo.ObtenerPorId(resultadoZona.Id);

                var resultado = _pokemonUserService.ObtenerPokemon(
                    Usuario.Id,
                    poke.numero_pokedex,
                    poke.Nombre,
                    poke.TipoPrincipal,
                    poke.TipoSecundario,
                    poke.EstadisticasBase[0]
                );

                resultadosMulti.Add(resultado);

               

                repoHist.RegistrarTirada(new HistoricoTirada
                {
                    UserId = Usuario.Id,
                    PokemonId = poke.numero_pokedex,
                    NombrePokemon = poke.Nombre,
                    Zona = NombreZona,
                    TipoTirada = "multi",
                    Fecha = DateTime.Now
                });
            }

            
            var ventanaResultados = new ResultadosMultiView(resultadosMulti);
            ventanaResultados.ShowDialog();
        }

        private void ActualizarFichas()
        {
            var userRepo = new UserRepository();
            Usuario = userRepo.GetUserById(Usuario.Id);
            Fichas = Usuario.FichasCasino;
        }



        //private void ProcesarResultado(PokemonZonaViewModel pokemon)
        //{
        //    var poke = _pokedexRepo.ObtenerPorId(pokemon.Id);

        //    if (poke == null)
        //    {
        //        MessageBox.Show("Error: Pokémon no encontrado en Pokédex.");
        //        return;
        //    }

        //    var resultado = _pokemonUserService.ObtenerPokemon(
        //        Usuario.Id,
        //        poke.Id,
        //        poke.Nombre,
        //        poke.TipoPrincipal,
        //        poke.TipoSecundario
        //    );

        //    MessageBox.Show(
        //        $"¡Has obtenido a {poke.Nombre}!\n" +
        //        $"Cantidad total: {resultado.Cantidad}\n" +
        //        $"Nivel actual: {resultado.Nivel}"
        //    );
        //}


        private void MostrarPokemon()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);

            if (zona == null)
            {
                System.Windows.MessageBox.Show(
                    $"No se encontró la zona '{NombreZona}'.",
                    "Zona no encontrada"
                );
                return;
            }

            if (zona.Pokemon == null || zona.Pokemon.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    $"La zona '{zona.Nombre}' no tiene Pokémon registrados.",
                    "Zona vacía"
                );
                return;
            }

            string lista = "";

            foreach (var p in zona.Pokemon)
            {
                var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);

                if (poke == null)
                {
                    lista += $"ID {p.numero_pokedex} (No encontrado en Pokédex) - Prob: {p.prob}%\n";
                    continue;
                }

                lista += $"{poke.Nombre} - Prob: {p.prob}%\n";
            }

            System.Windows.MessageBox.Show(
                lista,
                $"Pokémon disponibles en {zona.Nombre}"
            );
        }




        private void MostrarResultado(PokemonZonaViewModel pokemon)
        {
            // 1. Obtener el Pokémon real desde la Pokédex
            var poke = _pokedexRepo.ObtenerPorId(pokemon.Id);

            if (poke == null)
            {
                System.Windows.MessageBox.Show("Error: Pokémon no encontrado en Pokédex.");
                return;
            }

            // 2. Guardar o actualizar el Pokémon en la colección del usuario
            var resultado = _pokemonUserService.ObtenerPokemon(
                Usuario.Id,
                poke.numero_pokedex,
                poke.Nombre,
                poke.TipoPrincipal,
                poke.TipoSecundario,
                poke.EstadisticasBase[0]
            );

            // 3. Mostrar mensaje
            System.Windows.MessageBox.Show(
                $"¡Has obtenido a {poke.Nombre}!\n" +
                $"Cantidad total: {resultado.Cantidad}\n" +
                $"Nivel actual: {resultado.Nivel}",
                "Resultado del Gacha",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        }






        public void MostrarZonasBD()
        {
            var zonas = _zonaRepo.ObtenerTodas();

            if (zonas == null || zonas.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "No se encontraron zonas en la base de datos.",
                    "Zonas",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            string lista = string.Join("\n", zonas.Select(z => z.Nombre));

            System.Windows.MessageBox.Show(
                lista,
                "Zonas disponibles en la base de datos",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        }

        public void DebugBuscarZona()
        {
            var zona = _zonaRepo.ObtenerPorNombre(NombreZona);

            if (zona == null)
            {
                System.Windows.MessageBox.Show(
                    $"No se encontró la zona con el nombre EXACTO:\n\n\"{NombreZona}\"",
                    "Zona no encontrada",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            System.Windows.MessageBox.Show(
                $"Zona encontrada:\n\nNombre: {zona.Nombre}\nRegión: {zona.Region}\nTipo: {zona.Tipo}\nPokémon: {zona.Pokemon?.Count}",
                "Zona encontrada",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        }

        public void DebugMostrarIdYNombre()
{
    var zona = _zonaRepo.ObtenerPorNombre(NombreZona);

    if (zona == null)
    {
        System.Windows.MessageBox.Show("Zona no encontrada.");
        return;
    }

    if (zona.Pokemon == null || zona.Pokemon.Count == 0)
    {
        System.Windows.MessageBox.Show("La zona no tiene Pokémon.");
        return;
    }

    string lista = "";

    foreach (var p in zona.Pokemon)
    {
        lista += $"ID en Zona: {p.numero_pokedex} → ";

        var poke = _pokedexRepo.ObtenerPorId(p.numero_pokedex);

        if (poke == null)
        {
            lista += "NO ENCONTRADO EN POKEDEX\n";
        }
        else
        {
            lista += $"{poke.Nombre} (ID real: {poke.numero_pokedex})\n";
        }
    }

    System.Windows.MessageBox.Show(lista, "Relación Zona → Pokédex");
}


        public void DebugMostrarPokedex()
        {
            var todos = _pokedexRepo.ObtenerTodos();

            if (todos == null || todos.Count == 0)
            {
                System.Windows.MessageBox.Show("La Pokédex está vacía.");
                return;
            }

            string lista = "";

            foreach (var p in todos)
            {
                lista += $"ID {p.numero_pokedex} → {p.Nombre}\n";
            }

            System.Windows.MessageBox.Show(lista, "Pokédex completa");
        }


    }
}
