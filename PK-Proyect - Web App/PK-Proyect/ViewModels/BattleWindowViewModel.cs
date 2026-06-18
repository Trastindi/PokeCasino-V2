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
        public string PlayerHpText    => $"{_playerHp}/{_playerMaxHp}";

        // ─ Rival
        private string       _opponentName   = "Rival";
        private int          _opponentLevel  = 1;
        private BitmapImage? _opponentSprite;
        public string       OpponentName   { get => _opponentName;   set { _opponentName   = value; OnPropertyChanged(); } }
        public int          OpponentLevel  { get => _opponentLevel;  set { _opponentLevel  = value; OnPropertyChanged(); } }
        public BitmapImage? OpponentSprite { get => _opponentSprite; set { _opponentSprite = value; OnPropertyChanged(); } }
        public double OpponentHpPercent => _opponentMaxHp == 0 ? 0 : _opponentHp * 100.0 / _opponentMaxHp;
        public string OpponentHpText    => $"{_opponentHp}/{_opponentMaxHp}";

        // ─ Mensaje y turno
        private string _battleMessage = "Cargando batalla...";
        private bool   _isBattleActive;
        public string BattleMessage  { get => _battleMessage; set { _battleMessage = value; OnPropertyChanged(); } }
        public bool   IsBattleActive { get => _isBattleActive; set { _isBattleActive = value; OnPropertyChanged();
                                         ((RelayCommand)UseMoveCommand).RaiseCanExecuteChanged();
                                         ((RelayCommand)SwitchPokemonCommand).RaiseCanExecuteChanged(); } }

        // ─ Equipo y selección
        public ObservableCollection<PokemonEquipoItem> Equipo      { get; } = new();
        public ObservableCollection<string>            TurnSummary { get; } = new();
        public ObservableCollection<MoveModel>         PlayerMoves { get; } = new();

        private PokemonEquipoItem? _selectedPokemon;
        public PokemonEquipoItem? SelectedPokemon
        {
            get => _selectedPokemon;
            set { _selectedPokemon = value; OnPropertyChanged();
                  ((RelayCommand)ConfirmPickCommand).RaiseCanExecuteChanged();
                  ((RelayCommand)ConfirmSwitchCommand).RaiseCanExecuteChanged(); }
        }
        private bool CanConfirmPick => _selectedPokemon != null;

        // ─ Mensajes de fin
        private string _finishMessage = string.Empty;
        public string FinishMessage { get => _finishMessage; set { _finishMessage = value; OnPropertyChanged(); } }

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
                IniciarBatallaAsync();
        }

        // ─ Inicio: espera hasta que el propio equipo esté disponible (polling)
        private async void IniciarBatallaAsync()
        {
            BattleMessage = "Esperando que el rival envíe su equipo...";
            try
            {
                var deadline = DateTime.UtcNow.AddSeconds(120);
                BattleState? state = null;

                while (DateTime.UtcNow < deadline)
                {
                    state = await _battleService.GetBattleStateAsync(_battleId!);

                    if (state == null)
                    {
                        BattleMessage = "Error al cargar la batalla (no se pudo conectar con el servidor).";
                        return;
                    }

                    // Si ya tiene equipo propio deserializado, podemos continuar
                    if (state.HasTeam(_myPlayerId!))
                        break;

                    // Estados terminales: no esperar más
                    if (state.Status == "cancelled" || state.Status == "finished")
                    {
                        BattleMessage = "La batalla fue cancelada o ya ha terminado.";
                        return;
                    }

                    BattleMessage = $"Esperando confirmación del rival... (estado: {state.Status})";
                    await Task.Delay(2000);
                }

                if (state == null || !state.HasTeam(_myPlayerId!))
                {
                    BattleMessage = "Tiempo de espera agotado. El rival no confirmó a tiempo.";
                    return;
                }

                var myTeam = state.GetTeamOf(_myPlayerId!);
                Equipo.Clear();
                foreach (var pk in myTeam)
                    Equipo.Add(PokemonEquipoItem.FromBattlePokemon(pk));
                BattleMessage = "Elige tu Pokémon inicial.";
            }
            catch (Exception ex)
            {
                BattleMessage = $"Error al iniciar la batalla: {ex.Message}";
            }
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
            catch (Exception ex) { BattleMessage = $"Error: {ex.Message}"; }
        }

        // ─ Loop de polling de estado
        private async Task PollLoopAsync()
        {
            while (!_pollCts.Token.IsCancellationRequested)
            {
                try
                {
                    var state = await _battleService.GetBattleStateAsync(_battleId!);
                    if (state != null) ApplyState(state, fromPoll: true);
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
                    foreach (var m in serverMoves) PlayerMoves.Add(m);
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

            if (fromPoll && state.TurnLog?.Count > 0)
            {
                var hash = string.Join("|", state.TurnLog);
                if (hash != _lastSeenTurnLogHash)
                {
                    _lastSeenTurnLogHash = hash;
                    ShowTurnSummaryLines(state.TurnLog);
                }
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
            TurnSummary.Clear();
            foreach (var l in lines) TurnSummary.Add(l);
            ShowTurnSummary = true;
        }

        private void DismissTurnSummary()
        {
            ShowTurnSummary = false;
            TurnSummary.Clear();
        }

        // ─ Usar movimiento
        private async Task UseMoveAsync(MoveModel? move)
        {
            if (move == null || !IsBattleActive) return;
            IsBattleActive = false;
            ShowMoves      = false;
            ShowActions    = true;
            try
            {
                var result = await _battleService.UseMoveAsync(_battleId!, move.Name);
                if (result?.NewState != null) ApplyState(result.NewState);
            }
            catch (Exception ex) { BattleMessage = $"Error: {ex.Message}"; }
            finally { if (IsBattleActive == false && ShowBattle) IsBattleActive = true; }
        }

        // ─ Cambio de Pokémon
        private Task ShowSwitchPanel()
        {
            ShowBattle       = false;
            ShowPokemonPicker = true;
            return Task.CompletedTask;
        }

        private async Task ConfirmSwitchAsync()
        {
            if (_selectedPokemon == null) return;
            int idx = Equipo.IndexOf(_selectedPokemon);
            try
            {
                var ok = await _battleService.SwitchPokemonAsync(_battleId!, idx);
                if (!ok) { BattleMessage = "Error al cambiar Pokémon."; return; }
                ShowPokemonPicker = false;
                ShowBattle        = true;
            }
            catch (Exception ex) { BattleMessage = $"Error: {ex.Message}"; }
        }

        // ─ Placeholder sin servidor
        private void CargarEquipoPlaceholder()
        {
            BattleMessage = "Modo demo (sin servidor).";
            for (int i = 1; i <= 3; i++)
                Equipo.Add(new PokemonEquipoItem { Name = $"Pokémon {i}", Level = 5 });
        }

        private void ApplyDemoPokemon(PokemonEquipoItem pk)
        {
            PlayerName   = pk.Name;
            PlayerLevel  = pk.Level;
            _playerHp = _playerMaxHp = 100;
            OnPropertyChanged(nameof(PlayerHpPercent));
            OnPropertyChanged(nameof(PlayerHpText));
        }

        private static BitmapImage? LoadBitmap(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource      = new Uri(url, UriKind.Absolute);
                bmp.CacheOption    = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                return bmp;
            }
            catch { return null; }
        }

        public void Dispose() { _pollCts.Cancel(); _pollCts.Dispose(); }
    }
}
