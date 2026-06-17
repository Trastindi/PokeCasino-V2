using PK_Proyect.Commands;
using PK_Proyect.Models;
using PK_Proyect.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PK_Proyect.ViewModels
{
    public class MisMensajesViewModel
    {
        private readonly MensajeRepository _repo;

        /// <summary>Lista de mensajes recibidos por el usuario autenticado.</summary>
        public ObservableCollection<Mensaje> Mensajes { get; set; }

        /// <summary>Mensaje seleccionado en la lista (binding desde la View).</summary>
        public Mensaje MensajeSeleccionado { get; set; }

        /// <summary>
        /// Se dispara cuando el usuario acepta un desafío de batalla.
        /// Parámetro: battleId del mensaje seleccionado.
        /// </summary>
        public event Action<string> BatallaAceptada;

        public ICommand AceptarDesafioCommand { get; }

        public MisMensajesViewModel()
        {
            _repo    = new MensajeRepository();
            Mensajes = new ObservableCollection<Mensaje>();

            AceptarDesafioCommand = new RelayCommand(
                _ => AceptarDesafio(),
                _ => MensajeSeleccionado != null
                     && !string.IsNullOrEmpty(MensajeSeleccionado.TipoBatallaId)
            );

            _ = CargarMensajesAsync();
        }

        private void AceptarDesafio()
        {
            if (MensajeSeleccionado == null
                || string.IsNullOrEmpty(MensajeSeleccionado.TipoBatallaId))
            {
                MessageBox.Show("Selecciona un mensaje de desafío primero.");
                return;
            }

            // Notifica a la View con el battleId para que abra BattleWindowView
            BatallaAceptada?.Invoke(MensajeSeleccionado.TipoBatallaId);
        }

        private async Task CargarMensajesAsync()
        {
            var lista = await Task.Run(() =>
                _repo.GetMisMensajes()
                     .OrderByDescending(m => m.Fecha)
                     .ToList()
            );

            foreach (var m in lista)
                Mensajes.Add(m);
        }
    }
}
