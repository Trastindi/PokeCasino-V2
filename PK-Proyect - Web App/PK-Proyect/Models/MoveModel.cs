namespace PK_Proyect.Models
{
    /// <summary>
    /// Modelo que representa un movimiento de Pokémon en batalla.
    /// </summary>
    public class MoveModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Pp { get; set; }
        public int MaxPp { get; set; }
        public string Type { get; set; } // "Normal", "Fuego", etc.
        public int Power { get; set; }
        public int Accuracy { get; set; }
        public string Description { get; set; }

        public string PpText => $"{Pp}/{MaxPp}";
    }
}
