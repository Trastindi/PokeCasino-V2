using System.Collections.Generic;
using PK_Proyect.Models;

namespace PK_Proyect.Repositories
{
    public interface IZonaRepository
    {
        List<Zona> ObtenerTodas();
        Zona ObtenerPorNombre(string nombre);
    }
}
