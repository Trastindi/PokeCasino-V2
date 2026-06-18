using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PK_Proyect
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Excepciones no capturadas en hilos de fondo
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                string msg = args.ExceptionObject?.ToString() ?? "Error desconocido";
                try { File.WriteAllText("crash.log", $"[{DateTime.Now}]\n{msg}"); } catch { }
                MessageBox.Show(
                    $"Error crítico inesperado:\n\n{msg}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Excepciones en Tasks fire-and-forget (async void, _ = Task...)
            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                string msg = args.Exception?.ToString() ?? "Error desconocido en tarea";
                try { File.WriteAllText("crash_task.log", $"[{DateTime.Now}]\n{msg}"); } catch { }
                MessageBox.Show(
                    $"Error en tarea asíncrona:\n\n{args.Exception?.InnerException?.Message ?? msg}",
                    "Error de tarea", MessageBoxButton.OK, MessageBoxImage.Error);
                args.SetObserved(); // Evita que el proceso se cierre
            };

            // Excepciones en el hilo UI de WPF
            DispatcherUnhandledException += (s, args) =>
            {
                string msg = args.Exception?.ToString() ?? "Error desconocido en UI";
                try { File.WriteAllText("crash_ui.log", $"[{DateTime.Now}]\n{msg}"); } catch { }
                MessageBox.Show(
                    $"Error en la interfaz:\n\n{args.Exception?.Message ?? msg}",
                    "Error de UI", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Evita cierre
            };
        }
    }
}
