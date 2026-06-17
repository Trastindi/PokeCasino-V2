using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using PK_Proyect.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PK_Proyect.ViewModels
{
    public class StatItem
    {
        public string Nombre { get; set; }
        public int    Valor  { get; set; }
    }

    public class PokedexViewModel : INotifyPropertyChanged
    {
        // ── Colores por tipo ──────────────────────────────────────────────────────
        private static readonly Dictionary<string, (string bg, string fg)> TipoColores =
            new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
            {
                { "Bicho",    ("#729F3F", "#FFFFFF") },
                { "Dragón",   ("#53A4CF", "#FFFFFF") },
                { "Eléctrico",("#EED535", "#212121") },
                { "Lucha",    ("#D56723", "#FFFFFF") },
                { "Fuego",    ("#FD7D24", "#FFFFFF") },
                { "Volador",  ("#3DC7EF", "#212121") },
                { "Fantasma", ("#7B62A3", "#FFFFFF") },
                { "Planta",   ("#9BCC50", "#212121") },
                { "Tierra",   ("#F7DE3F", "#212121") },
                { "Hielo",    ("#51C4E7", "#212121") },
                { "Normal",   ("#A4ACAF", "#212121") },
                { "Veneno",   ("#B97FC9", "#FFFFFF") },
                { "Psíquico", ("#F366B9", "#FFFFFF") },
                { "Roca",     ("#A38C21", "#FFFFFF") },
                { "Agua",     ("#4592C4", "#FFFFFF") },
            };

        // ── Repositorios y estado ─────────────────────────────────────────────────
        private readonly PokedexRepository      _repo;
        private readonly PokemonUserRepository  _pokemonUserRepo;
        private readonly string                 _userId;

        // IDs de Pokémon que el usuario ha obtenido
        private HashSet<int> _obtenidos = new HashSet<int>();

        private List<CroppedBitmap> _sprites;        // sprites normales
        private List<CroppedBitmap> _hiddenSprites;  // sprites ocultos

        private const int MaxNumber  = 151;
        private const int SpriteCols = 15;
        private const int SpriteRows = 11;

        private int    _currentNo   = 1;
        private string _inputNo     = "001";
        private string _screenMode  = "SPRITE";  // SPRITE | DESCRIPCION | REGION

        private string _tipo1       = string.Empty;
        private string _tipo2       = string.Empty;
        private string _descripcion = string.Empty;
        private string _region      = string.Empty;
        private string _nombre      = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        // ── Commands ──────────────────────────────────────────────────────────────
        public ICommand ClickUpCommand    { get; }
        public ICommand ClickDownCommand  { get; }
        public ICommand ClickLeftCommand  { get; }
        public ICommand ClickRightCommand { get; }
        public ICommand KeypadCommand     { get; }
        public ICommand SearchCommand     { get; }
        public ICommand ClearCommand      { get; }

        // ── Constructor ───────────────────────────────────────────────────────────
        public PokedexViewModel(string userId)
        {
            _userId          = userId;
            _repo            = new PokedexRepository();
            _pokemonUserRepo = new PokemonUserRepository();

            // Sprites normales (pokedexicons.png)
            try { _sprites = SpriteHelper.SplitSprites("/Images/pokedexicons.png", SpriteCols, SpriteRows); }
            catch { _sprites = new List<CroppedBitmap>(); }

            // Sprites ocultos (pokedexhiddenicons.png)
            try { _hiddenSprites = SpriteHelper.SplitSprites("/Images/pokedexhiddenicons.png", SpriteCols, SpriteRows); }
            catch { _hiddenSprites = new List<CroppedBitmap>(); }

            ClickUpCommand    = new RelayCommand(_ => OnClickUp());
            ClickDownCommand  = new RelayCommand(_ => OnClickDown());
            ClickLeftCommand  = new RelayCommand(_ => OnClickLeft());
            ClickRightCommand = new RelayCommand(_ => OnClickRight());
            KeypadCommand     = new RelayCommand(p => OnKeypad(p?.ToString()));
            SearchCommand     = new RelayCommand(_ => OnSearch());
            ClearCommand      = new RelayCommand(_ => { InputPokedexNo = string.Empty; });

            _ = InicializarAsync();
        }

        // Carga primero los obtenidos y luego el primer Pokémon
        private async Task InicializarAsync()
        {
            await CargarObtenidosAsync();
            await LoadPokemonAsync(_currentNo);
        }

        private async Task CargarObtenidosAsync()
        {
            var lista = await Task.Run(() => _pokemonUserRepo.GetPokemonsByUser(_userId));
            _obtenidos = new HashSet<int>(lista.Select(p => p.PokemonId));
        }

        // ¿El pokémon actual ha sido obtenido por el usuario?
        private bool EsObtenido => _obtenidos.Contains(_currentNo);

        // ── Propiedades expuestas al XAML ─────────────────────────────────────────

        public string InputPokedexNo
        {
            get => _inputNo;
            set { _inputNo = value; Notify(); }
        }

        public string NombreDisplay  => EsObtenido && !string.IsNullOrEmpty(_nombre) ? _nombre : "????";
        public string NumeroDisplay  => $"N.º {_currentNo:000}";
        public string Descripcion    => EsObtenido && !string.IsNullOrEmpty(_descripcion) ? _descripcion : "????";
        public string Region         => EsObtenido && !string.IsNullOrEmpty(_region) ? _region : "????";

        public CroppedBitmap CurrentSprite
        {
            get
            {
                int idx = (_currentNo - 1).Clamp(0, MaxNumber - 1);

                if (EsObtenido)
                {
                    if (_sprites != null && idx < _sprites.Count) return _sprites[idx];
                }
                else
                {
                    if (_hiddenSprites != null && idx < _hiddenSprites.Count) return _hiddenSprites[idx];
                }
                return null;
            }
        }

        // Visibilidad de paneles
        public Visibility SpritePanelVisible      => _screenMode == "SPRITE"      ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DescripcionPanelVisible => _screenMode == "DESCRIPCION" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RegionPanelVisible      => _screenMode == "REGION"      ? Visibility.Visible : Visibility.Collapsed;

        // Tipo 1 — oculto si no ha sido obtenido
        public string     Tipo1           => EsObtenido ? _tipo1 : string.Empty;
        public Visibility Tipo1Visible    => EsObtenido && !string.IsNullOrEmpty(_tipo1) ? Visibility.Visible : Visibility.Collapsed;
        public Brush      Tipo1Background => BrushFromTipo(_tipo1, isBg: true);
        public Brush      Tipo1Foreground => BrushFromTipo(_tipo1, isBg: false);

        // Tipo 2 — oculto si no ha sido obtenido
        public string     Tipo2           => EsObtenido ? _tipo2 : string.Empty;
        public Visibility Tipo2Visible    => EsObtenido && !string.IsNullOrEmpty(_tipo2) ? Visibility.Visible : Visibility.Collapsed;
        public Brush      Tipo2Background => BrushFromTipo(_tipo2, isBg: true);
        public Brush      Tipo2Foreground => BrushFromTipo(_tipo2, isBg: false);

        // Stats — vacías si no obtenido
        public ObservableCollection<StatItem> Stats { get; } = new ObservableCollection<StatItem>();

        // ── Navegación ────────────────────────────────────────────────────────────

        private void OnClickRight()
        {
            _currentNo = _currentNo == MaxNumber ? 1 : Math.Min(_currentNo + 1, MaxNumber);
            _inputNo   = _currentNo.ToString("000");
            Notify(nameof(InputPokedexNo));
            _ = LoadPokemonAsync(_currentNo);
        }

        private void OnClickLeft()
        {
            _currentNo = _currentNo == 1 ? MaxNumber : Math.Max(_currentNo - 1, 1);
            _inputNo   = _currentNo.ToString("000");
            Notify(nameof(InputPokedexNo));
            _ = LoadPokemonAsync(_currentNo);
        }

        private void OnClickUp()
        {
            _screenMode = _screenMode switch
            {
                "SPRITE"      => "DESCRIPCION",
                "DESCRIPCION" => "REGION",
                _             => "SPRITE"
            };
            NotifyPanels();
        }

        private void OnClickDown()
        {
            _screenMode = _screenMode switch
            {
                "SPRITE"  => "REGION",
                "REGION"  => "DESCRIPCION",
                _         => "SPRITE"
            };
            NotifyPanels();
        }

        private void OnKeypad(string digit)
        {
            if (string.IsNullOrEmpty(digit)) return;
            var clean = (_inputNo + digit).TrimStart('0');
            if (clean.Length > 3) clean = clean.Substring(clean.Length - 3);
            _inputNo = clean.PadLeft(3, '0');
            Notify(nameof(InputPokedexNo));
        }

        private void OnSearch()
        {
            if (!int.TryParse(_inputNo, out int n)) return;
            _currentNo = Math.Clamp(n, 1, MaxNumber);
            _inputNo   = _currentNo.ToString("000");
            Notify(nameof(InputPokedexNo));
            _ = LoadPokemonAsync(_currentNo);
        }

        // ── Carga de datos ────────────────────────────────────────────────────────

        private async Task LoadPokemonAsync(int numero)
        {
            // Siempre cargamos del repo (para tenerlo listo si se desbloqueara)
            // pero solo exponemos los datos si EsObtenido
            try
            {
                var pokemon = await Task.Run(() => _repo.ObtenerPorId(numero));
                if (pokemon != null)
                {
                    _nombre      = pokemon.Nombre          ?? string.Empty;
                    _tipo1       = pokemon.TipoPrincipal   ?? string.Empty;
                    _tipo2       = pokemon.TipoSecundario  ?? string.Empty;
                    _descripcion = pokemon.Descripcion     ?? string.Empty;
                    _region      = pokemon.Region          ?? string.Empty;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Stats.Clear();
                        // Solo añadimos stats si el usuario lo ha obtenido
                        if (EsObtenido && pokemon.EstadisticasBase != null)
                        {
                            var eb = pokemon.EstadisticasBase;
                            Stats.Add(new StatItem { Nombre = "PS",        Valor = eb.Ps });
                            Stats.Add(new StatItem { Nombre = "Ataque",    Valor = eb.Ataque });
                            Stats.Add(new StatItem { Nombre = "Defensa",   Valor = eb.Defensa });
                            Stats.Add(new StatItem { Nombre = "Sp.Atq",    Valor = eb.AtaqueEspecial });
                            Stats.Add(new StatItem { Nombre = "Sp.Def",    Valor = eb.DefensaEspecial });
                            Stats.Add(new StatItem { Nombre = "Velocidad", Valor = eb.Velocidad });
                        }
                    });
                }
                else
                {
                    _nombre = _tipo1 = _tipo2 = _descripcion = _region = string.Empty;
                    Application.Current.Dispatcher.Invoke(() => Stats.Clear());
                }
            }
            catch
            {
                _nombre = _tipo1 = _tipo2 = _descripcion = _region = string.Empty;
            }

            Application.Current.Dispatcher.Invoke(NotifyAll);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private Brush BrushFromTipo(string tipo, bool isBg)
        {
            if (string.IsNullOrEmpty(tipo)) return Brushes.Transparent;
            if (TipoColores.TryGetValue(tipo, out var colors))
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(isBg ? colors.bg : colors.fg));
            return isBg ? Brushes.Gray : Brushes.White;
        }

        private void NotifyPanels()
        {
            Notify(nameof(SpritePanelVisible));
            Notify(nameof(DescripcionPanelVisible));
            Notify(nameof(RegionPanelVisible));
        }

        private void NotifyAll()
        {
            Notify(nameof(CurrentSprite));
            Notify(nameof(NombreDisplay));
            Notify(nameof(NumeroDisplay));
            Notify(nameof(Descripcion));
            Notify(nameof(Region));
            Notify(nameof(Tipo1));
            Notify(nameof(Tipo2));
            Notify(nameof(Tipo1Background));
            Notify(nameof(Tipo1Foreground));
            Notify(nameof(Tipo2Background));
            Notify(nameof(Tipo2Foreground));
            Notify(nameof(Tipo1Visible));
            Notify(nameof(Tipo2Visible));
            NotifyPanels();
        }

        private void Notify([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    internal static class IntClampExt
    {
        public static int Clamp(this int v, int min, int max)
            => v < min ? min : v > max ? max : v;
    }
}
