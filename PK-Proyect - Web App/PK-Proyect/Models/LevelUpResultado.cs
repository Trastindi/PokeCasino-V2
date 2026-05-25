namespace PK_Proyect.Models
{
    /// <summary>
    /// Resultado devuelto por PokemonUserService.ObtenerPokemon().
    /// Contiene el pokemon actualizado y flags para que el ViewModel
    /// sepa si debe abrir ventanas adicionales.
    /// </summary>
    public class LevelUpResultado
    {
        /// <summary>Estado final del PokemonUser (ya persistido en BD).</summary>
        public PokemonUser Pokemon { get; set; }

        // --- Movimiento nuevo ---
        /// <summary>Nombre del movimiento aprendido, o null si no aprendió ninguno.</summary>
        public string MovimientoAprendido { get; set; }
        /// <summary>
        /// true  → hueco libre, el movimiento ya fue añadido al moveset.
        /// false → moveset lleno, el ViewModel debe preguntar qué borrar.
        /// Solo relevante cuando MovimientoAprendido != null.
        /// </summary>
        public bool MovimientoAprendidoDirectamente { get; set; }

        // --- Evolución ---
        /// <summary>true si el Pokémon evolucionó en esta subida de nivel.</summary>
        public bool Evoluciono { get; set; }
        /// <summary>Nombre de la forma evolucionada, o null si no evolucionó.</summary>
        public string NombreEvolucion { get; set; }
        /// <summary>
        /// Movimiento que la evolución puede aprender a nivel 1 y que
        /// todavía no estaba en el moveset. null si no aplica.
        /// </summary>
        public string MovimientoEvolucion { get; set; }
        public bool MovimientoEvolucionDirectamente { get; set; }
    }
}
