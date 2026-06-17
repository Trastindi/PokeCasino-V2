using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.Collections.ObjectModel;
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
        private string _battleId;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Visual
        public Brush BackgroundBrush { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
        public Brush ButtonBackground { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
        public Brush ButtonForeground { get; } = Brushes.White;

        // Jugador
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

        // Rival
        private string _opponentName = "Eevee";
        public string OpponentName { get => _opponentName; set { _opponentName = value; OnPropertyChanged(); } }

        private int _opponentLevel = 5;
        public int OpponentLevel { get => _opponentLevel; set { _opponentLevel = value; OnPropertyChanged(); } }

        private int _opponentHp = 20;
        private int _opponentMaxHp = 20;
        public double OpponentHpPercent => _opponentMaxHp == 0 ? 0 : (_opponentHp * 100.0 / _opponentMaxHp);
        public string OpponentHpText => $"{_opponentHp}/{_opponentMaxHp} PS";

        private ImageSource? _opponentSprite;
        public ImageSource? OpponentSprite { get => _opponentSprite; set { _opponentSprite = value; OnPropertyChanged(); } }

        // Movimientos
        private ObservableCollection<MoveModel> _playerMoves;
        public ObservableCollection<MoveModel> PlayerMoves
        {
            get => _playerMoves;
            set { _playerMoves = value; OnPropertyChanged(); }
        }

        // Commands — lambdas con parámetro _ para coincidir con Func<object,Task>
        public RelayCommand OpenMovesCommand    { get; }
        public RelayCommand UseItemCommand      { get; }
        public RelayCommand SwitchPokemonCommand { get; }
        public RelayCommand RunCommand          { get; }

        // Estado
        private string _battleMessage = string.Empty;
        public string BattleMessage { get => _battleMessage; set { _battleMessage = value; OnPropertyChanged(); } }

        private bool _isBattleActive = true;
        public bool IsBattleActive { get => _isBattleActive; set { _isBattleActive = value; OnPropertyChanged(); } }

        public BattleWindowViewModel(IBattleService battleService, string battleId = null)
        {
            _battleService = battleService;
            _battleId      = battleId;

            PlayerMoves = new ObservableCollection<MoveModel>();

            // Lambdas con parámetro _ para Func<object, Task> / Func<object, bool>
            OpenMovesCommand     = new RelayCommand(_ => { OpenMoves();    return Task.CompletedTask; });
            UseItemCommand       = new RelayCommand(_ => { UseItem();      return Task.CompletedTask; });
            SwitchPokemonCommand = new RelayCommand(_ => { SwitchPokemon(); return Task.CompletedTask; });
            RunCommand           = new RelayCommand(async _ => await TryRunAsync());

            if (!string.IsNullOrWhiteSpace(battleId))
                _ = LoadBattleDataAsync(battleId);
            else
                InitializeWithPlaceholders();
        }

        private async Task LoadBattleDataAsync(string battleId)
        {
            try
            {
                var battleData = await _battleService.GetBattleDataAsync(battleId);
                if (battleData != null)
                {
                    PlayerName   = battleData.PlayerPokemonName;
                    PlayerLevel  = battleData.PlayerLevel;
                    _playerHp    = battleData.PlayerHp;
                    _playerMaxHp = battleData.PlayerMaxHp;

                    OpponentName   = battleData.OpponentPokemonName;
                    OpponentLevel  = battleData.OpponentLevel;
                    _opponentHp    = battleData.OpponentHp;
                    _opponentMaxHp = battleData.OpponentMaxHp;

                    if (!string.IsNullOrWhiteSpace(battleData.PlayerSpriteUrl))
                        PlayerSprite = LoadBitmap(battleData.PlayerSpriteUrl);
                    if (!string.IsNullOrWhiteSpace(battleData.OpponentSpriteUrl))
                        OpponentSprite = LoadBitmap(battleData.OpponentSpriteUrl);

                    OnPropertyChanged(nameof(PlayerHpPercent));
                    OnPropertyChanged(nameof(PlayerHpText));
                    OnPropertyChanged(nameof(OpponentHpPercent));
                    OnPropertyChanged(nameof(OpponentHpText));
                }
                else
                    InitializeWithPlaceholders();
            }
            catch (Exception ex)
            {
                BattleMessage = $"Error al cargar batalla: {ex.Message}";
                InitializeWithPlaceholders();
            }
        }

        private void InitializeWithPlaceholders()
        {
            BattleMessage = "Batalla iniciada. ¡Selecciona tu acción!";
            PlayerMoves.Clear();
            PlayerMoves.Add(new MoveModel { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",     Power = 40 });
            PlayerMoves.Add(new MoveModel { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",      Power = 90 });
            PlayerMoves.Add(new MoveModel { Name = "Rayo",        Pp = 10, MaxPp = 10, Type = "Eléctrico",  Power = 90 });
            PlayerMoves.Add(new MoveModel { Name = "Protección",  Pp = 5,  MaxPp = 5,  Type = "Normal",     Power = 0  });
        }

        private void OpenMoves()    => BattleMessage = "Selecciona un movimiento.";
        private void UseItem()      => BattleMessage = "Abriendo bolsa...";
        private void SwitchPokemon() => BattleMessage = "Selecciona Pokémon.";

        private async Task TryRunAsync()
        {
            BattleMessage = "Intentas huir...";
            await Task.Delay(300);
            if (new Random().Next(100) < 50)
            {
                BattleMessage = "¡Huiste con éxito!";
                IsBattleActive = false;
            }
            else
            {
                BattleMessage = "¡No pudiste huir!";
            }
        }

        public void ApplyDamageToOpponent(int damage)
        {
            _opponentHp = Math.Max(0, _opponentHp - damage);
            OnPropertyChanged(nameof(OpponentHpPercent));
            OnPropertyChanged(nameof(OpponentHpText));
            if (_opponentHp <= 0) { BattleMessage = $"¡{PlayerName} ganó la batalla!"; IsBattleActive = false; }
        }

        public void ApplyDamageToPlayer(int damage)
        {
            _playerHp = Math.Max(0, _playerHp - damage);
            OnPropertyChanged(nameof(PlayerHpPercent));
            OnPropertyChanged(nameof(PlayerHpText));
            if (_playerHp <= 0) { BattleMessage = $"¡{OpponentName} ganó la batalla!"; IsBattleActive = false; }
        }

        public void UseMove(MoveModel move)
        {
            if (move == null || !IsBattleActive) return;
            if (move.Pp <= 0) { BattleMessage = "No hay PP disponible para ese movimiento."; return; }
            move.Pp--;
            BattleMessage = $"¡{PlayerName} usa {move.Name}!";
        }

        private BitmapImage? LoadBitmap(string uri)
        {
            try { return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute)); }
            catch { return null; }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
