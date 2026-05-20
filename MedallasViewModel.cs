using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace PK_Proyect.ViewModels
{
    public class MedallasViewModel
    {
        private readonly PokemonUserRepository _repo;

        public ObservableCollection<MedallaItem> Medallas { get; set; }

      
        private readonly Dictionary<string, int> requisitos = new()
        {
            { "Normal", 5 },
            { "Fuego", 5 },
            { "Agua", 5 },
            { "Planta", 3 },
            { "Eléctrico", 5 },
            { "Hielo", 5 },
            { "Lucha", 5 },
            { "Veneno", 5 },
            { "Tierra", 5 },
            { "Volador", 5 },
            { "Psíquico", 5 },
            { "Bicho", 5 },
            { "Roca", 5 },
            { "Fantasma", 5 },
            { "Dragón", 5 }
        };

        public MedallasViewModel(string userId)
        {
            _repo = new PokemonUserRepository();
            Medallas = new ObservableCollection<MedallaItem>();

            CargarMedallas(userId);
        }

        private void CargarMedallas(string userId)
        {
            foreach (var tipo in requisitos.Keys)
            {
                int cantidad = _repo.CountByType(userId, tipo);
                int requerido = requisitos[tipo];

                bool desbloqueada = cantidad >= requerido;

                Medallas.Add(new MedallaItem
                {
                    Nombre = tipo,
                    Icono = desbloqueada ? "★" : "?", // Más adelante será una imagen
                    ColorFondo = desbloqueada ? Brushes.LightGoldenrodYellow : Brushes.Gray,
                    Tooltip = desbloqueada
                        ? $"Medalla de {tipo} conseguida."
                        : $"Necesitas {requerido} Pokémon de tipo {tipo}."
                });
            }
        }
    }

    public class MedallaItem
    {
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public Brush ColorFondo { get; set; }
        public string Tooltip { get; set; }
    }
}
