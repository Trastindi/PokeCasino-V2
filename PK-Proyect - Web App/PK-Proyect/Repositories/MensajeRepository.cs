using PK_Proyect.Models;
using System.Collections.Generic;

namespace PK_Proyect.Repositories
{
    public class MensajeRepository
    {
        /// <summary>Devuelve todos los mensajes recibidos por el usuario autenticado.</summary>
        public List<Mensaje> GetMisMensajes()
            => ApiClient.Get<List<Mensaje>>("/messages/mis_mensajes");

        /// <summary>Elimina un mensaje por su Id.</summary>
        public void EliminarMensaje(string id)
            => ApiClient.Delete($"/messages/{id}");

        /// <summary>
        /// Acepta o rechaza un desafío de batalla.
        /// POST /battle_requests/{msgId}/respond  { "accepted": true/false }
        /// Devuelve true si el servidor responde 2xx.
        /// </summary>
        public bool ResponderDesafio(string msgId, bool accepted)
        {
            try
            {
                // El servidor devuelve { "msg": "...", "battle_id": "..." } o { "msg": "..." }
                // No necesitamos el cuerpo, solo que no lance excepción (200/OK).
                ApiClient.Post<System.Text.Json.JsonElement>(
                    $"/battle_requests/{msgId}/respond",
                    new { accepted });
                return true;
            }
            catch { return false; }
        }
    }
}
