using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class MisMensajesViewModel
    {
        private readonly MensajeRepository _repo;

        public ObservableCollection<Mensaje> Mensajes { get; set; }

        public MisMensajesViewModel()
        {
            _repo = new MensajeRepository();
            Mensajes = new ObservableCollection<Mensaje>();

            _ = CargarMensajesAsync();
        }

        private async Task CargarMensajesAsync()
        {
            var lista = await Task.Run(() =>
                _repo.GetMisMensajes()
                    .OrderByDescending(m => m.Fecha)
                    .ToList()
            );

            foreach (var m in lista)
            {
                System.Console.WriteLine($"Mensaje de: {m.Remitente}, Fecha: {m.Fecha}, Leido: {m.Leido}");
                Mensajes.Add(m);
            }
        }
    }
}
