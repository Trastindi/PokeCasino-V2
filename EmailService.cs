using System.Net;
using System.Net.Mail;
using System.Windows;


namespace PK_Proyect.Services
{
    public static class EmailService
    {
        public static void Send(string destinatario, string asunto, string mensaje)
        {
            string texto =
$@"────────────────────────────────────────
        📡 MENSAJE DEL PROFESOR OAK 📡
────────────────────────────────────────

De: Prof. Oak <prof.oak@pokemon.com>
Para: {destinatario}

Asunto: {asunto}

{mensaje}

────────────────────────────────────────
Si tienes dudas, pasa por el laboratorio.
¡Sigue entrenando, joven entrenador!
────────────────────────────────────────";

            MessageBox.Show(
                texto,
                "📨 Email enviado",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
