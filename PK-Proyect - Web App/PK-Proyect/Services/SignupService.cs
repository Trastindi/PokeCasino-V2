namespace PK_Proyect.Services
{
    /// <summary>
    /// OBSOLETO — el registro de nuevos entrenadores lo gestiona TrainerService
    /// llamando a POST /auth/register en el servidor Flask.
    /// Esta clase se conserva únicamente para no romper referencias de compilación.
    /// </summary>
    [System.Obsolete("Usar TrainerService.CreateUser() en su lugar.")]
    public class SignupService
    {
        public void RegisterNewTrainer(string name, string gender) { }
    }
}
