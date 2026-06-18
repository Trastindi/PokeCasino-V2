using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PK_Proyect.ViewModels
{
    public class BattleWindowViewModel : ViewModelBase, IDisposable
    {
        // ─ Dependencias
        private readonly IBattleService _battleService;
        private readonly string?        _battleId;
        private readonly string?        _myPlayerId;
        private          CancellationTokenSource _pollCts = new();

        public Window? OwnerWindow { get; set; }

        // ─ HP internos
        private int _playerHp,   _playerMaxHp   = 1;
        private int _opponentHp, _opponentMaxHp = 1;

        // ─ Visibilidad de paneles
        private bool _showPokemonPicker = true;
        private bool _showBattle;
        private bool _showForcedSwitch;
        private bool _showFinished;
        private bool _showTurnSummary;

        public bool ShowPokemonPicker { get => _showPokemonPicker; set { _showPokemonPicker = value; OnPropertyChanged(); } }
        public bool ShowBattle        { get => _showBattle;        set { _showBattle        = value; OnPropertyChanged(); } }
        public bool ShowForcedSwitch  { get => _showForcedSwitch;  set { _showForcedSwitch  = value; OnPropertyChanged(); } }
        public bool ShowFinished      { get => _showFinished;      set { _showFinished      = value; OnPropertyChanged(); } }
        public bool ShowTurnSummary   { get => _showTurnSummary;   set { _showTurnSummary   = value; OnPropertyChanged(); } }

        // ─ Sub-paneles
        private bool _showActions = true;
        private bool _showMoves;
        public bool ShowActions { get => _showActions; set { _showActions = value; OnPropertyChanged(); } }
        public bool ShowMoves   { get => _showMoves;   set { _showMoves   = value; OnPropertyChanged(); } }

        // ─ Jugador
        private string       _playerName   = "Tu Pokémon";
        private int          _playerLevel  = 1;
        private BitmapImage? _playerSprite;
        public string       PlayerName   { get => _playerName;   set { _playerName   = value; OnPropertyChanged(); } }
        public int          PlayerLevel  { get => _playerLevel;  set { _playerLevel  = value; OnPropertyChanged(); } }
        public BitmapImage? PlayerSprite { get => _playerSprite; set { _playerSprite = value; OnPropertyChanged(); } }
        public double PlayerHpPercent => _playerMaxHp == 0 ? 0 : _playerHp * 100.0 / _playerMaxHp;
        public string PlayerHpText    => $"{_playerHp} / {_playerMaxHp}";

        // ─ Rival
        private string       _opponentName   = "Rival";
        private int          _opponentLevel  = 1;
        private BitmapImage? _opponentSprite;
        public string       OpponentName   { get => _opponentName;   set { _opponentName   = value; OnPropertyChanged(); } }
        public int          OpponentLevel  { get => _opponentLevel;  set { _opponentLevel  = value; OnPropertyChanged(); } }
        public BitmapImage? OpponentSprite { get => _opponentSprite; set { _opponentSprite = value; OnPropertyChanged(); } }
        public double OpponentHpPercent => _opponentMaxHp == 0 ? 0 : _opponentHp * 100.0 / _opponentMaxHp;
        public string OpponentHpText    => $"{_opponentHp} / {_opponentMaxHp}";

        // ─ Colecciones
        public ObservableCollection<MoveModel>         PlayerMoves     { get; } = new();
        public ObservableCollection<PokemonEquipoItem> Equipo          { get; } = new();
        public ObservableCollection<string>            TurnLog         { get; } = new();
        public ObservableCollection<string>            LastTurnSummary { get; } = new();

        private PokemonEquipoItem? _selectedPokemon;
        public  PokemonEquipoItem? SelectedPokemon
        {
            get => _selectedPokemon;
            set
            {
                if (_selectedPokemon != null) _selectedPokemon.IsSelected = false;
                _selectedPokemon = value;
                if (_selectedPokemon != null) _selectedPokemon.IsSelected = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirmPick));
            }
        }
        public bool CanConfirmPick => _selectedPokemon != null && !_selectedPokemon.EstaDerrotado;

        private string _battleMessage = "Elige tu Pokémon inicial.";
        public  string BattleMessage  { get => _battleMessage; set { _battleMessage = value; OnPropertyChanged(); } }

        private bool _isBattleActive;
        public  bool IsBattleActive  { get => _isBattleActive; set { _isBattleActive = value; OnPropertyChanged(); } }

        private string _finishMessage = string.Empty;
        public  string FinishMessage  { get => _finishMessage; set { _finishMessage = value; OnPropertyChanged(); } }

        // ─ Comandos
        public ICommand ConfirmPickCommand        { get; }
        public ICommand OpenMovesCommand          { get; }
        public ICommand BackToActionsCommand      { get; }
        public ICommand UseMoveCommand            { get; }
        public ICommand SwitchPokemonCommand      { get; }
        public ICommand ConfirmSwitchCommand      { get; }
        public ICommand UseItemCommand            { get; }
        public ICommand RunCommand                { get; }
        public ICommand CloseCommand              { get; }
        public ICommand DismissTurnSummaryCommand { get; }

        // ─ Constructor
        public BattleWindowViewModel(IBattleService battleService,
                                     string?        battleId   = null,
                                     string?        myPlayerId = null)
        {
            _battleService = battleService;
            _battleId      = battleId;
            _myPlayerId    = myPlayerId;

            // RelayCommand acepta Action<object> — lambdas síncronas
            ConfirmPickCommand        = new RelayCommand(_ => ConfirmPickAsync(),               _ => CanConfirmPick);
            OpenMovesCommand          = new RelayCommand(_ => { ShowActions = false; ShowMoves = true; });
            BackToActionsCommand      = new RelayCommand(_ => { ShowMoves = false; ShowActions = true; });
            UseMoveCommand            = new RelayCommand(p  => { _ = UseMoveAsync(p as MoveModel); },  _ => IsBattleActive);
            SwitchPokemonCommand      = new RelayCommand(_ => { _ = ShowSwitchPanel(); },               _ => IsBattleActive);
            ConfirmSwitchCommand      = new RelayCommand(_ => { _ = ConfirmSwitchAsync(); },            _ => CanConfirmPick);
            UseItemCommand            = new RelayCommand(_ => MessageBox.Show("Sin objetos."));
            RunCommand                = new RelayCommand(_ => OwnerWindow?.Close());
            CloseCommand              = new RelayCommand(_ => OwnerWindow?.Close());
            DismissTurnSummaryCommand = new RelayCommand(_ => DismissTurnSummary());

            if (string.IsNullOrEmpty(_battleId))
                CargarEquipoPlaceholder();
            else
                IniciarBatallaAsync();   // async void — no asignar con _ =
        }

        // ─ Inicio (async void: fire-and-forget controlado desde constructor)
        private async void IniciarBatallaAsync()
        {
            try
            {
                var state = await _battleService.GetBattleStateAsync(_battleId!);
                if (state == null) { BattleMessage = "Error al cargar la batalla."; return; }
                var myTeam = state.GetTeamOf(_myPlayerId!);
                Equipo.Clear();
                foreach (var pk in myTeam)
                    Equipo.Add(PokemonEquipoItem.FromBattlePokemon(pk));
                BattleMessage = "Elige tu Pokémon inicial.";
            }
            catch (Exception ex) { BattleMessage = $"Error: {ex.Message}"; }
        }

        // ─ Confirmar Pokémon
        private async void ConfirmPickAsync()
        {
            if (_selectedPokemon == null) return;
            if (string.IsNullOrEmpty(_battleId))
            {
                ApplyDemoPokemon(_selectedPokemon);
                ShowPokemonPicker = false;
                ShowBattle        = true;
                IsBattleActive    = true;
                return;
            }
            int idx = Equipo.IndexOf(_selectedPokemon);
            try
            {
                var ok = await _battleService.ChoosePokemonAsync(_battleId!, idx);
                if (!ok) { BattleMessage = "Error al elegir Pokémon. Inténtalo de nuevo."; return; }
                ShowPokemonPicker = false;
                ShowBattle        = true;
                IsBattleActive    = true;
                _ = PollLoopAsync();
            }
            catch (Exception ex) { BattleMessage = ex.Message; }
        }

        // ─ Polling
        private async Task PollLoopAsync()
        {
            while (!_pollCts.IsCancellationRequested)
            {
                try
                {
                    var state = await _battleService.GetBattleStateAsync(_battleId!);
                    if (state != null)
                        Application.Current.Dispatcher.Invoke(() => ApplyState(state, fromPoll: true));
                    if (state?.Status == "finished") break;
                }
                catch { }
                await Task.Delay(2000, _pollCts.Token).ContinueWith(_ => { });
            }
        }

        // ─ Aplicar estado
        private string _lastSeenTurnLogHash = string.Empty;

        private void ApplyState(BattleState state, bool fromPoll = false)
        {
            var myPk  = state.GetActivePokemonOf(_myPlayerId!);
            var oppPk = state.GetActivePokemonOf(state.GetOpponentId(_myPlayerId!));

            if (myPk != null)
            {
                PlayerName   = myPk.Name;
                PlayerLevel  = myPk.Level;
                _playerHp    = myPk.HpCurrent;
                _playerMaxHp = myPk.HpMax;
                PlayerSprite = LoadBitmap(myPk.SpriteUrl);
                OnPropertyChanged(nameof(PlayerHpPercent));
                OnPropertyChanged(nameof(PlayerHpText));

                var serverMoves   = myPk.Moves ?? new();
                bool movesChanged = serverMoves.Count != PlayerMoves.Count
                    || serverMoves.Where((m, i) => i < PlayerMoves.Count &&
                                                   (PlayerMoves[i].Name != m.Name ||
                                                    PlayerMoves[i].Pp   != m.Pp)).Any();
                if (movesChanged)
                {
                    PlayerMoves.Clear();
                    foreach (var m in serverMoves)
                        PlayerMoves.Add(new MoveModel
                        {
                            Name  = m.Name,  Type  = m.Type,
                            Power = m.Power, Pp    = m.Pp,
                            MaxPp = m.MaxPp
                        });
                }
            }

            if (oppPk != null)
            {
                OpponentName   = oppPk.Name;
                OpponentLevel  = oppPk.Level;
                _opponentHp    = oppPk.HpCurrent;
                _opponentMaxHp = oppPk.HpMax;
                OpponentSprite = LoadBitmap(oppPk.SpriteUrl);
                OnPropertyChanged(nameof(OpponentHpPercent));
                OnPropertyChanged(nameof(OpponentHpText));
            }

            // Resumen de turno
            if (state.TurnLog != null && state.TurnLog.Count > 0)
            {
                string logHash = string.Join("|", state.TurnLog);
                if (logHash != _lastSeenTurnLogHash)
                {
                    _lastSeenTurnLogHash = logHash;
                    ShowTurnSummaryLines(state.TurnLog);
                }
                foreach (var msg in state.TurnLog)
                    if (!TurnLog.Contains(msg)) TurnLog.Add(msg);
            }

            switch (state.Status)
            {
                case "choosing_action":
                    IsBattleActive   = true;
                    ShowBattle       = true;
                    ShowForcedSwitch = false;
                    break;

                case "waiting_switch":
                    if (state.SwitchTurnId == _myPlayerId)
                    {
                        BattleMessage    = "Tu Pokémon fue derrotado. Elige otro.";
                        ShowBattle       = false;
                        ShowForcedSwitch = true;
                        ShowTurnSummary  = false;
                    }
                    break;

                case "finished":
                    IsBattleActive   = false;
                    ShowBattle       = false;
                    ShowForcedSwitch = false;
                    ShowTurnSummary  = false;
                    ShowFinished     = true;
                    FinishMessage    = state.WinnerId == _myPlayerId ? "¡Has ganado!" : "Has perdido...";
                    break;
            }
        }

        // ─ Resumen de turno
        private void ShowTurnSummaryLines(List<string> lines)
        {
            LastTurnSummary.Clear();
            foreach (var line in lines) LastTurnSummary.Add(line);
            ShowTurnSummary = true;
            IsBattleActive  = false;
        }

        private void DismissTurnSummary()
        {
            ShowTurnSummary = false;
            if (ShowBattle) IsBattleActive = true;
        }

        // ─ Usar movimiento
        private async Task UseMoveAsync(MoveModel? move)
        {
            if (move == null || string.IsNullOrEmpty(_battleId)) return;
            IsBattleActive = false;
            ShowMoves      = false;
            ShowActions    = true;
            try
            {
                var result = await _battleService.UseMoveAsync(_battleId!, move.Name);
                if (result?.NewState != null)
                    Application.Current.Dispatcher.Invoke(() => ApplyState(result.NewState));
            }
            catch (Exception ex) { TurnLog.Add($"Error: {ex.Message}"); }
        }

        // ─ Cambio de Pokémon
        private Task ShowSwitchPanel()
        {
            BattleMessage    = "Elige el Pokémon que enviarás.";
            ShowBattle       = false;
            ShowForcedSwitch = true;
            return Task.CompletedTask;
        }

        private async Task ConfirmSwitchAsync()
        {
            if (_selectedPokemon == null) return;
            int idx = Equipo.IndexOf(_selectedPokemon);
            if (!string.IsNullOrEmpty(_battleId))
                await _battleService.SwitchPokemonAsync(_battleId!, idx);
            ShowForcedSwitch = false;
            ShowBattle       = true;
            IsBattleActive   = true;
            SelectedPokemon  = null;
        }

        // ─ Placeholder (modo demo)
        private void ApplyDemoPokemon(PokemonEquipoItem pk)
        {
            PlayerName   = pk.Nombre;
            PlayerLevel  = pk.Nivel;
            _playerHp    = pk.HpActual;
            _playerMaxHp = pk.HpMax;
            PlayerSprite = LoadBitmap(pk.ImagenUrl);
            OnPropertyChanged(nameof(PlayerHpPercent));
            OnPropertyChanged(nameof(PlayerHpText));
            PlayerMoves.Clear();
            foreach (var m in pk.Movimientos) PlayerMoves.Add(m);
            OpponentName   = "Rival";
            OpponentLevel  = 5;
            _opponentHp    = 20;
            _opponentMaxHp = 20;
            OnPropertyChanged(nameof(OpponentHpPercent));
            OnPropertyChanged(nameof(OpponentHpText));
            OpponentSprite = LoadBitmap(
                "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/back/1.png");
        }

        private void CargarEquipoPlaceholder()
        {
            Equipo.Clear();
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId     = "25",
                Nombre        = "Pikachu",
                Nivel         = 5,
                TipoPrincipal = "Eléctrico",
                HpActual      = 19,
                HpMax         = 19,
                ImagenUrl     = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
                Movimientos   = new List<MoveModel>
                {
                    new() { Name = "Impactrueno", Pp = 30, MaxPp = 30, Type = "Eléctrico", Power = 40  },
                    new() { Name = "Placaje",     Pp = 35, MaxPp = 35, Type = "Normal",    Power = 40  },
                    new() { Name = "Cola Férrea", Pp = 15, MaxPp = 15, Type = "Acero",     Power = 100 },
                    new() { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal",    Power = 0   }
                }
            });
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId     = "4",
                Nombre        = "Charmander",
                Nivel         = 5,
                TipoPrincipal = "Fuego",
                HpActual      = 22,
                HpMax         = 22,
                ImagenUrl     = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png",
                Movimientos   = new List<MoveModel>
                {
                    new() { Name = "Arañazo",     Pp = 35, MaxPp = 35, Type = "Normal", Power = 40 },
                    new() { Name = "Ascuas",      Pp = 25, MaxPp = 25, Type = "Fuego",  Power = 40 },
                    new() { Name = "Gruñido",     Pp = 40, MaxPp = 40, Type = "Normal", Power = 0  },
                    new() { Name = "Lanzallamas", Pp = 15, MaxPp = 15, Type = "Fuego",  Power = 90 }
                }
            });
            Equipo.Add(new PokemonEquipoItem
            {
                PokemonId     = "7",
                Nombre        = "Squirtle",
                Nivel         = 5,
                TipoPrincipal = "Agua",
                HpActual      = 20,
                HpMax         = 20,
                ImagenUrl     = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/7.png",
                Movimientos   = new List<MoveModel>
                {
                    new() { Name = "Pistola Agua", Pp = 25, MaxPp = 25, Type = "Agua",   Power = 40 },
                    new() { Name = "Placaje",      Pp = 35, MaxPp = 35, Type = "Normal", Power = 40 },
                    new() { Name = "Defensa",      Pp = 40, MaxPp = 40, Type = "Normal", Power = 0  },
                    new() { Name = "Burbuja",      Pp = 30, MaxPp = 30, Type = "Agua",   Power = 20 }
                }
            });
        }

        // ─ Helpers
        private static BitmapImage? LoadBitmap(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource     = new Uri(url);
                bmp.CacheOption   = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        public void Dispose()
        {
            _pollCts.Cancel();
            _pollCts.Dispose();
        }
    }
}
