using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace PK_Proyect.Models
{
    /// <summary>
    /// Movimiento de un Pokémon. Compartido entre BattlePokemon (servidor)
    /// y PokemonEquipoItem (UI local).
    /// INotifyPropertyChanged permite actualizar los PP en tiempo real
    /// cuando el servidor devuelve el nuevo estado tras un turno.
    /// </summary>
    public class MoveModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? p = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("power")]
        public int Power { get; set; }

        private int _pp;
        [JsonPropertyName("pp")]
        public int Pp
        {
            get => _pp;
            set { _pp = value; Notify(); Notify(nameof(PpText)); Notify(nameof(IsUsable)); }
        }

        [JsonPropertyName("max_pp")]
        public int MaxPp { get; set; }

        /// <summary>"10/15" para mostrar en el botón de movimiento.</summary>
        public string PpText => $"{Pp}/{MaxPp}";

        /// <summary>False cuando PP = 0; deshabilita el botón en la View.</summary>
        public bool IsUsable => Pp > 0;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;   // Físico / Especial / Estado

        [JsonPropertyName("accuracy")]
        public int Accuracy { get; set; } = 100;
    }
}
