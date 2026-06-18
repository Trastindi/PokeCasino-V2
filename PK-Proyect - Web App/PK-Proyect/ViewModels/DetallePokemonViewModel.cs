using PK_Proyect.Models;
using System.Collections.Generic;
using System.Linq;

namespace PK_Proyect.ViewModels
{
    /// <summary>
    /// Wrapper sobre PokemonUser que expone propiedades calculadas
    /// listas para bindear directamente desde DetallePokemonView.xaml.
    /// </summary>
    public class DetallePokemonViewModel
    {
        private readonly PokemonUser _pokemon;

        public DetallePokemonViewModel(PokemonUser pokemon)
        {
            _pokemon = pokemon;
        }

        // ── Datos básicos ────────────────────────────────────────────────────
        public string Nombre         => _pokemon.Nombre;
        public int    NumeroPokedex  => _pokemon.numero_pokedex;
        public int    Nivel          => _pokemon.Nivel;
        public string TipoPrincipal  => _pokemon.TipoPrincipal;
        public string TipoSecundario => string.IsNullOrWhiteSpace(_pokemon.TipoSecundario)
                                            ? "—"
                                            : _pokemon.TipoSecundario;
        public string Habilidad      => string.IsNullOrWhiteSpace(_pokemon.AbilityId)
                                            ? "—"
                                            : _pokemon.AbilityId;
        public string Objeto         => string.IsNullOrWhiteSpace(_pokemon.ItemId)
                                            ? "—"
                                            : _pokemon.ItemId;
        public string FechaObtenido  => _pokemon.FechaObtenido.ToString("dd/MM/yyyy");
        public int    CurrentHp      => _pokemon.CurrentHp;
        public string Status         => string.IsNullOrWhiteSpace(_pokemon.Status)
                                            ? "—"
                                            : _pokemon.Status;

        // ── Sprite (PokeAPI) ─────────────────────────────────────────────────
        /// <summary>
        /// URL del sprite frontal oficial desde PokeAPI.
        /// Usa numero_pokedex para construirla (no depende de datos del servidor).
        /// </summary>
        public string SpriteUrl =>
            $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{_pokemon.numero_pokedex}.png";

        // ── Estadísticas base (Dictionary<string,int> con claves en minúscula) ──
        /// <summary>PS / HP</summary>
        public int StatPs              => GetStat("ps");
        /// <summary>Ataque</summary>
        public int StatAtaque          => GetStat("ataque");
        /// <summary>Defensa</summary>
        public int StatDefensa         => GetStat("defensa");
        /// <summary>Ataque especial</summary>
        public int StatAtaqueEspecial  => GetStat("ataque_especial");
        /// <summary>Defensa especial</summary>
        public int StatDefensaEspecial => GetStat("defensa_especial");
        /// <summary>Velocidad</summary>
        public int StatVelocidad       => GetStat("velocidad");

        // ── Moveset ──────────────────────────────────────────────────────────
        /// <summary>Cadena de movimientos separados por coma.</summary>
        public string MovimientosTexto =>
            (_pokemon.MoveSet == null || !_pokemon.MoveSet.Any())
                ? "—"
                : string.Join(", ", _pokemon.MoveSet);

        /// <summary>Lista de movimientos para ItemsControl.</summary>
        public IEnumerable<string> Movimientos =>
            (_pokemon.MoveSet != null && _pokemon.MoveSet.Any())
                ? _pokemon.MoveSet
                : new List<string> { "—" };

        // ── Helpers ──────────────────────────────────────────────────────────
        private int GetStat(string clave)
        {
            if (_pokemon.estadisticas_base != null &&
                _pokemon.estadisticas_base.TryGetValue(clave, out int val))
                return val;
            return 0;
        }
    }
}
