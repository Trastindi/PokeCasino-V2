using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using PK_Proyect.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PK_Proyect.ViewModels
{
    /// <summary>
    /// ViewModel principal del combate.
    /// Máquina de estados guiada por el campo "status" de la BD:
    ///
    ///   ready            → panel selección inicial de Pokémon
    ///   choosing_action  → combate activo (movimientos o cambio voluntario)
    ///   waiting_switch   → cambio forzado (el Pokémon activo fue derrotado)
    ///   finished         → mensaje victoria/derrota, polling parado
    ///
    /// NOTA: ChoosePokemonAsync y SwitchPokemonAsync reciben el índice (int)
    ///       dentro del equipo, no el pokemon_id string.
    ///       El servidor extrae el jugador del JWT — no se envía player_id.
    /// </summary>
    public class BattleWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        // ── Dependencias ───────────────────────────────────────────────
        private readonly IBattleService _battleService;
        private string   _battleId;
        private string   _myPlayerId;
        private CancellationTokenSource _pollCts;

        public event PropertyChangedEventHandler? PropertyChanged;
        public Window OwnerWindow { get; set; }

        // ── Visual fija ───────────────────────────────────────────────
        public Brush BackgroundBrush  { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3C04A"));
        public Brush ButtonBackground { get; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2B2B2B"));
        public Brush ButtonForeground { get; } = Brushes.White;

        // ── Estado de la máquina ──────────────────────────────────────────
        private string _battleStatus = "ready";
        public  string BattleStatus
        {
            get => _battleStatus;
            private set
            {
                if (_battleStatus == value) return;
                _battleStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowPokemonPicker));
                OnPropertyChanged(nameof(ShowBattle));
                OnPropertyChanged(nameof(ShowForcedSwitch));
                OnPropertyChanged(nameof(ShowFinished));
                OnPropertyChanged(nameof(IsBattleActive));
            }
        }

        public bool ShowPokemonPicker => BattleStatus == "ready";
        public bool ShowBattle        => BattleStatus == "choosing_action";
        public bool ShowForcedSwitch  => BattleStatus == "waiting_switch";
        public bool ShowFinished      => BattleStatus == "finished";
        public bool IsBattleActive    => BattleStatus == "choosing_action";

        private bool _showMoves;
        public  bool ShowMoves
        {
            get => _showMoves;
            set { _showMoves = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowActions)); }
        }
        public bool ShowActions => !_showMoves;

        // ── Datos del jugador ────────────────────────────────────────────
        private string _playerName = "Pikachu";
        public  string PlayerName  { get => _playerName; set { _playerName = value; OnPropertyChanged(); } }

        private int _playerLevel = 5;
        public  int PlayerLevel    { get => _playerLevel; set { _playerLevel = value; OnPropertyChanged(); } }

        private int _playerHp = 19, _playerMaxHp = 19;
        public  double PlayerHpPercent => _playerMaxHp == 0 ? 0 : _playerHp * 100.0 / _playerMaxHp;
        public  string PlayerHpText    => $"{_playerHp}/{_playerMaxHp} PS";

        private ImageSource? _playerSprite;
        public  ImageSource? PlayerSprite { get => _playerSprite; set { _playerSprite = value; OnPropertyChanged(); } }

        // ── Datos del rival ─────────────────────────────────────────────
        private string _opponentName = "Eevee";
        public  string OpponentName  { get => _opponentName; set { _opponentName = value; OnPropertyChanged(); } }

        private int _opponentLevel = 5;
        public  int OpponentLevel    { get => _opponentLevel; set { _opponentLevel = value; OnPropertyChanged(); } }

        private int _opponentHp = 20, _opponentMaxHp = 20;
        public  double OpponentHpPercent => _opponentMaxHp == 0 ? 0 : _opponentHp * 100.0 / _opponentMaxHp;
        public  string OpponentHpText    => $"{_opponentHp}/{_opponentMaxHp} PS";

        private ImageSource? _opponentSprite;
        public  ImageSource? OpponentSprite { get => _opponentSprite; set { _opponentSprite = value; OnPropertyChanged(); } }

        // ── Movimientos del Pokémon activo ─────────────────────────────────
        public ObservableCollection<MoveModel> PlayerMoves { get; } = new();

        // ── Log del turno ────────────────────────────────────────────────
        public ObservableCollection<string> TurnLog { get; } = new();

        private string _battleMessage = string.Empty;
        public  string BattleMessage  { get => _battleMessage; set { _battleMessage = value; OnPropertyChanged(); } }

        private string _finishMessage = string.Empty;
        public  string FinishMessage  { get => _finishMessage; set { _finishMessage = value; OnPropertyChanged(); } }

        // ── Comandos ───────────────────────────────────────────────────
        public RelayCommand OpenMovesCommand      { get; }
        public RelayCommand BackToActionsCommand  { get; }
        public RelayCommand UseItemCommand        { get; }
        public RelayCommand SwitchPokemonCommand  { get; }
        public RelayCommand RunCommand            { get; }
        public RelayCommand UseMoveCommand        { get; }
        public RelayCommand ConfirmPickCommand    { get; }
        public RelayCommand ConfirmSwitchCommand  { get; }
        public RelayCommand CloseCommand          { get; }

        private PokemonEquipoItem _pickedPokemon;
        public  PokemonEquipoItem PickedPokemon
        {
            get => _pickedPokemon;
            set { _pickedPokemon = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConfirmPick)); }
        }
        public bool CanConfirmPick => PickedPokemon != null;

        public ObservableCollection<PokemonEquipoItem> Equipo { get; } = new();

        // ── Constructor ───────────────────────────────────────────────
        public BattleWindowViewModel(IBattleService battleService,
                                     string battleId   = null,
                                     string myPlayerId = null)
        {
            _battleService = battleService;
            _battleId      = battleId;
            _myPlayerId    = myPlayerId;

            OpenMovesCommand    = new RelayCommand(_ => { ShowMoves = true;  BattleMessage = "¿Qué movimiento usará?"; return Task.CompletedTask; });
            BackToActionsCommand= new RelayCommand(_ => { ShowMoves = false; BattleMessage = "Selecciona una acción.";  return Task.CompletedTask; });
            UseItemCommand      = new RelayCommand(_ => { BattleMessage = "La bolsa aún no está disponible.";          return Task.CompletedTask; });
            CloseCommand        = new RelayCommand(_ => { StopPolling(); OwnerWindow?.Close(); return Task.CompletedTask; });

            UseMoveCommand = new RelayCommand(async param =>
            {
                if (param is not MoveModel move) return;
                ShowMoves = false;
                BattleMessage = $"Usando {move.Name}...";
                await SendMoveAsync(move);
            });

            SwitchPokemonCommand = new RelayCommand(_ =>
            {
                OpenSwitchDialog(forced: false);
                return Task.CompletedTask;
            });

            RunCommand = new RelayCommand(async _ => await TryRunAsync());

            // Confirmar elección inicial (ready): busca el índice en el equipo
            ConfirmPickCommand = new RelayCommand(async _ =>
            {
                if (PickedPokemon == null) return;
                await ConfirmInitialPickAsync();
            });

            // Confirmar cambio forzado (waiting_switch): busca el índice en el equipo
            ConfirmSwitchCommand = new RelayCommand(async _ =>
            {
                if (PickedPokemon == null) return;
                await ConfirmForcedSwitchAsync();
            });

            CargarEquipoPlaceholder();

            if (!string.IsNullOrWhiteSpace(battleId))
                _ = StartPollingAsync();
            else
                InitializeWithPlaceholders();
        }

        // ── Polling ──────────────────────────────────────────────────────

        private async Task StartPollingAsync()
        {
            _pollCts = new CancellationTokenSource();
            var token = _pollCts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var state = await _battleService.GetBattleStateAsync(_battleId);
                    if (state != null)
                        await Application.Current.Dispatcher.InvokeAsync(() => ApplyState(state));

                    if (BattleStatus == "finished") break;
                }
                catch { /* ignorar errores de red transitorios */ }

                await Task.Delay(2000, token).ContinueWith(_ => { });
            }
        }

        public void StopPolling() => _pollCts?.Cancel();

        // ── Aplicar snapshot del servidor ───────────────────────────────────

        private void ApplyState(BattleState state)
        {
            var myPk = state.Player1Id == _myPlayerId ? state.Player1Pokemon : state.Player2Pokemon;
            var opPk = state.Player1Id == _myPlayerId ? state.Player2Pokemon : state.Player1Pokemon;

            if (myPk != null)
            {
                PlayerName   = myPk.Name;
                PlayerLevel  = myPk.Level;
                _playerHp    = myPk.HpCurrent;
                _playerMaxHp = myPk.HpMax;
                PlayerSprite = LoadBitmap(myPk.SpriteUrl);
                OnPropertyChanged(nameof(PlayerHpPercent));
                OnPropertyChanged(nameof(PlayerHpText));

                var serverMoves = myPk.Moves ?? new();
                bool movesChanged = serverMoves.Count != PlayerMoves.Count
                    || serverMoves.Any((m, i) => PlayerMoves[i].Name != m.Name || PlayerMoves[i].Pp != m.Pp);

                if (movesChanged)
                {
                    PlayerMoves.Clear();
                    foreach (var m in serverMoves)
                        PlayerMoves.Add(new MoveModel { Name = m.Name, Type = m.Type, Power = m.Power, Pp = m.Pp, MaxPp = m.MaxPp });
                }
            }

            if (opPk != null)
            {
                OpponentName   = opPk.Name;
                OpponentLevel  = opPk.Level;
                _opponentHp    = opPk.HpCurrent;
                _opponentMaxHp = opPk.HpMax;
                OpponentSprite = LoadBitmap(opPk.SpriteUrl);
                OnPropertyChanged(nameof(OpponentHpPercent));
                OnPropertyChanged(nameof(OpponentHpText));
            }

            if (state.TurnLog?.Count > 0)
            {
                foreach (var line in state.TurnLog)
                    if (!TurnLog.Contains(line))
                        TurnLog.Add(line);

                BattleMessage = state.TurnLog[^1];
            }

            BattleStatus = state.Status ?? BattleStatus;

            if (state.Status == "finished")
            {
                StopPolling();
                FinishMessage = state.WinnerId == _myPlayerId
                    ? "¡Has ganado el combate!"
                    : "¡Has perdido el combate...";
            }

            if (state.Status == "waiting_switch" && state.SwitchTurnId == _myPlayerId)
                BattleMessage = "¡Tu Pokémon fue derrotado! Elige otro.";
        }

        // ── Acciones de turno ─────────────────────────────────────────────

        /// <summary>
        /// Busca el índice del Pokémon elegido en el equipo cargado y llama a
        /// ChoosePokemonAsync(battleId, index) — app.py espera pokemon_index int.
        /// </summary>
        private async Task ConfirmInitialPickAsync()
        {
            if (string.IsNullOrWhiteSpace(_battleId)) return;

            int idx = Equipo.IndexOf(PickedPokemon);
            if (idx < 0)
            {
                BattleMessage = "Error: Pokémon no encontrado en el equipo.";
                return;
            }

            var ok = await _battleService.ChoosePokemonAsync(_battleId, idx);
            BattleMessage = ok
                ? $"¡Adelante, {PickedPokemon.Nombre}!"
                : "Error al elegir Pokémon, inténtalo de nuevo.";
        }

        /// <summary>
        /// Busca el índice del Pokémon en el equipo y envía la acción de cambio
        /// vía SwitchPokemonAsync(battleId, index).
        /// </summary>
        private async Task ConfirmForcedSwitchAsync()
        {
            if (string.IsNullOrWhiteSpace(_battleId)) return;

            int idx = Equipo.IndexOf(PickedPokemon);
            if (idx < 0)
            {
                BattleMessage = "Error: Pokémon no encontrado en el equipo.";
                return;
            }

            var ok = await _battleService.SwitchPokemonAsync(_battleId, idx);
            BattleMessage = ok
                ? $"¡Adelante, {PickedPokemon.Nombre}!"
                : "Error al cambiar Pokémon.";
        }

        private async Task SendMoveAsync(MoveModel move)
        {
            if (string.IsNullOrWhiteSpace(_battleId)) return;
            var result = await _battleService.UseMoveAsync(_battleId, move.Name);
            if (result == null) return;

            if (result.Log?.Count > 0)
            {
                foreach (var line in result.Log)
                    TurnLog.Add(line);
                BattleMessage = result.Log[^1];
            }
        }

        private void OpenSwitchDialog(bool forced)
        {
            var vm  = new SwitchBattlePokemonViewModel();
            var win = new SwitchBattlePokemonWindow(vm) { Owner = OwnerWindow };
            if (win.ShowDialog() == true && vm.SelectedPokemon != null)
            {
                if (!forced)
                {
                    int idx = Equipo.IndexOf(vm.SelectedPokemon);
                    if (idx >= 0)
                        _ = _battleService.SwitchPokemonAsync(_battleId, idx);
                }

                PlayerName   = vm.SelectedPokemon.Nombre;
                PlayerLevel  = vm.SelectedPokemon.Nivel;
                _playerHp    = vm.SelectedPokemon.HpActual;
                _playerMaxHp = vm.SelectedPokemon.HpMax;
                PlayerSprite = LoadBitmap(vm.SelectedPokemon.ImagenUrl);
                OnPropertyChanged(nameof(PlayerHpPercent));
                OnPropertyChanged(nameof(PlayerHpText));
                BattleMessage = $"¡Adelante, {PlayerName}!";
                PlayerMoves.Clear();
                if (vm.SelectedPokemon.Movimientos != null)
                    foreach (var m in vm.SelectedPokemon.Movimientos)
                        PlayerMoves.Add(m);
            }
        }

        private async Task TryRunAsync()
        {
            var r = MessageBox.Show("¿Abandonar el combate?", "Abandonar",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            BattleMessage = "Intentas huir...";
            await Task.Delay(300);
            if (new Random().Next(100) < 50)
            {
                BattleMessage = "¡Huiste con éxito!";
                StopPolling();
                OwnerWindow?.Close();
            }
            else
                BattleMessage = "¡No pudiste huir!";
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private void InitializeWithPlaceholders()
        {
            BattleMessage = "Modo demo — sin batalla real.";
            PlayerMoves.Clear();
            PlayerMoves.Add(new MoveModel { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",    Power = 40 });
            PlayerMoves.Add(new MoveModel { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",     Power = 90 });
            PlayerMoves.Add(new MoveModel { Name = "Rayo",        Pp = 10, MaxPp = 10, Type = "Eléctrico", Power = 90 });
            PlayerMoves.Add(new MoveModel { Name = "Protección",  Pp = 5,  MaxPp = 5,  Type = "Normal",    Power = 0  });
        }

        private void CargarEquipoPlaceholder()
        {
            Equipo.Clear();
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId = "25", Nombre = "Pikachu", Nivel = 5, TipoPrincipal = "Eléctrico",
                HpActual = 19, HpMax = 19,
                ImagenUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
                Movimientos = new() {
                    new MoveModel { Name = "Impactrueno", Pp = 30, MaxPp = 30, Type = "Eléctrico", Power = 40 },
                    new MoveModel { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",    Power = 40 },
                    new MoveModel { Name = "Cola Férrea", Pp = 15, MaxPp = 15, Type = "Acero",     Power = 100 },
                    new MoveModel { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal",    Power = 0 }
                }
            });
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId = "4", Nombre = "Charmander", Nivel = 5, TipoPrincipal = "Fuego",
                HpActual = 22, HpMax = 22,
                ImagenUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png",
                Movimientos = new() {
                    new MoveModel { Name = "Arañazo",     Pp = 35, MaxPp = 35, Type = "Normal", Power = 40 },
                    new MoveModel { Name = "Ascuas",      Pp = 25, MaxPp = 25, Type = "Fuego",  Power = 40 },
                    new MoveModel { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal", Power = 0 },
                    new MoveModel { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",  Power = 90 }
                }
            });
        }

        private BitmapImage? LoadBitmap(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return null;
            try { return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute)); }
            catch { return null; }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose() => StopPolling();
    }
}
