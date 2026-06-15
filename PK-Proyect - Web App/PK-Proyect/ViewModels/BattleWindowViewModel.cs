using PK_Proyect.Commands;
using PK_Proyect.Services;
using PK_Proyect.Utils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PK_Proyect.ViewModels
{
    public class BattleWindowViewModel : INotifyPropertyChanged
    {
        private readonly IBattleService _battleService;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Visual / resources
        public Brush BackgroundBrush { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
        public Brush ButtonBackground { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
        public Brush ButtonForeground { get; } = Brushes.White;

        // Jugador (info mostrada encima del menú, a la derecha del sprite)
        private string _playerName = "Pikachu";
        public string PlayerName { get => _playerName; set { _playerName = value; OnPropertyChanged(); } }

        private int _playerLevel = 5;
        public int PlayerLevel { get => _playerLevel; set { _playerLevel = value; OnPropertyChanged(); } }

        private int _playerHp = 19;
        private int _playerMaxHp = 19;
        public double PlayerHpPercent => _playerMaxHp == 0 ? 0 : (_playerHp * 100.0 / _playerMaxHp);
        public string PlayerHpText => $"{_playerHp}/{_playerMaxHp} PS";

        private ImageSource? _playerSprite;
        public ImageSource? PlayerSprite { get => _playerSprite; set { _playerSprite = value; OnPropertyChanged(); } }

        // Rival (info mostrada a la izquierda del sprite, arriba-derecha)
        private string _opponentName = "Eevee";
        public string OpponentName { get => _opponentName; set { _opponentName = value; OnPropertyChanged(); } }

        private int _opponentLevel = 5;
        public int OpponentLevel { get => _opponentLevel; set { _opponentLevel = value; OnPropertyChanged(); } }

        private int _opponentHp = 20;
        private int _opponentMaxHp = 20;
        public double OpponentHpPercent => _opponentMaxHp == 0 ? 0 : (_opponentHp * 100.0 / _opponentMaxHp);

        private ImageSource? _opponentSprite;
        public ImageSource? OpponentSprite { get => _opponentSprite; set { _opponentSprite = value; OnPropertyChanged(); } }

        // Commands (bound in XAML)
        public RelayCommand OpenMovesCommand { get; }
        public RelayCommand UseItemCommand { get; }
        public RelayCommand SwitchPokemonCommand { get; }
        public RelayCommand RunCommand { get; }

        public BattleWindowViewModel(IBattleService battleService)
        {
            _battleService = battleService;

            // Commands: minimal implementations so UI responds
            OpenMovesCommand = new RelayCommand(() => { OpenMoves(); return Task.CompletedTask; });
            UseItemCommand = new RelayCommand(() => { UseItem(); return Task.CompletedTask; });
            SwitchPokemonCommand = new RelayCommand(() => { SwitchPokemon(); return Task.CompletedTask; });
            RunCommand = new RelayCommand(async () => await TryRunAsync());

            // Optional: load placeholder sprites if you want defaults
            //PlayerSprite = LoadBitmap("pack://application:,,,/Resources/pikachu.png");
            //OpponentSprite = LoadBitmap("pack://application:,,,/Resources/eevee.png");
        }

        // Example action methods
        private void OpenMoves()
        {
            // placeholder: set a message or open a moves panel in your app
            BattleMessage = "Selecciona un movimiento.";
        }

        private void UseItem()
        {
            BattleMessage = "Abriendo bolsa...";
        }

        private void SwitchPokemon()
        {
            BattleMessage = "Selecciona Pokémon.";
        }

        private async Task TryRunAsync()
        {
            BattleMessage = "Intentas huir...";
            await Task.Delay(300);
            BattleMessage = "Huiste con éxito.";
        }

        // Optional public methods to update HP from game logic
        public void ApplyDamageToOpponent(int damage)
        {
            _opponentHp = Math.Max(0, _opponentHp - damage);
            OnPropertyChanged(nameof(OpponentHpPercent));
        }

        public void ApplyDamageToPlayer(int damage)
        {
            _playerHp = Math.Max(0, _playerHp - damage);
            OnPropertyChanged(nameof(PlayerHpPercent));
            OnPropertyChanged(nameof(PlayerHpText));
        }

        // Battle message (optional, not shown in the provided XAML but useful)
        private string _battleMessage = string.Empty;
        public string BattleMessage { get => _battleMessage; set { _battleMessage = value; OnPropertyChanged(); } }

        // Helper to load images if needed
        private BitmapImage? LoadBitmap(string uri)
        {
            try
            {
                return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
