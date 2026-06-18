using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Representa un Pokémon del equipo del jugador en la UI de selección
    /// (pickers de combate: ready y waiting_switch).
    /// Implementa INotifyPropertyChanged para que el resalte de selección
    /// funcione en los ItemsControl sin código extra en el ViewModel.
    /// </summary>
    public class PokemonEquipoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        // ── Identidad ─────────────────────────────────────────────────────────────

        public string PokemonId     { get; set; } = string.Empty;
        public string Nombre        { get; set; } = string.Empty;
        public int    Nivel         { get; set; } = 5;
        public string TipoPrincipal { get; set; } = string.Empty;
        public string? TipoSecundario { get; set; }

        // ── HP ────────────────────────────────────────────────────────────────────

        private int _hpActual;
        public  int HpActual
        {
            get => _hpActual;
            set
            {
                _hpActual = value;
                Notify();
                Notify(nameof(HpPercent));
                Notify(nameof(EstaDerrotado));
            }
        }

        public int HpMax { get; set; } = 1;

        /// <summary>0-100, listo para un ProgressBar.</summary>
        public double HpPercent => HpMax == 0 ? 0 : HpActual * 100.0 / HpMax;

        /// <summary>True si el Pokémon no puede combatir (HP = 0).</summary>
        public bool EstaDerrotado => HpActual <= 0;

        // ── Visuales ──────────────────────────────────────────────────────────────

        public string ImagenUrl { get; set; } = string.Empty;

        // ── Movimientos ───────────────────────────────────────────────────────────

        public List<MoveModel> Movimientos { get; set; } = new();

        // ── Estado de selección UI ────────────────────────────────────────────────

        private bool _isSelected;
        public  bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Notify(); }
        }

        // ── Factory desde BattlePokemon (respuesta del servidor) ──────────────────

        public static PokemonEquipoItem FromBattlePokemon(BattlePokemon bp) =>
            new()
            {
                PokemonId  = bp.PokemonId,
                Nombre     = bp.Name,
                Nivel      = bp.Level,
                HpActual   = bp.HpCurrent,
                HpMax      = bp.HpMax,
                ImagenUrl  = bp.SpriteUrl,
                Movimientos = bp.Moves ?? new()
            };
    }
}
