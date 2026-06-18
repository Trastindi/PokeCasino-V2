using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public class ZonaRepository : IZonaRepository
    {
        public List<Zona> ObtenerTodas()
            => ApiClient.Get<List<Zona>>("/zonas");

        public Zona ObtenerPorNombre(string nombre)
        {
            try
            {
                return ApiClient.Get<Zona>($"/zonas/{nombre}");
            }
            catch
            {
                return null;
            }
        }
    }
}
