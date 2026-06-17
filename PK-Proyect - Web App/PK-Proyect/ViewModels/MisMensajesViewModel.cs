using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PK_Proyect.ViewModels
{
    public class MisMensajesViewModel
    {
        private readonly MensajeRepository _repo;

        public ObservableCollection<Mensaje> Mensajes { get; set; }

        public MisMensajesViewModel(string userId)
        {
            _repo = new MensajeRepository();
            Mensajes = new ObservableCollection<Mensaje>();

            // Cargar de forma asincrónica para no bloquear el UI
            _ = CargarMensajesAsync(userId);
        }

        private async Task CargarMensajesAsync(string userId)
        {
            var lista = await Task.Run(() =>
                _repo.GetMensajesByUser(userId)
                    .OrderByDescending(m => m.Fecha)
                    .ToList()
            );

            foreach (var m in lista)
            {
                System.Console.WriteLine($"Mensaje de: {m.Remitente}, Fecha: {m.Fecha}, Leído: {m.Leido}");
                Mensajes.Add(m);
            }
        }
    }
}
