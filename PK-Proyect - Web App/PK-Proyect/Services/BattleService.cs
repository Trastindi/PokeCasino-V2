//using System;
//using System.Threading.Tasks;

//namespace PK_Proyect.Services
//{
//    /// <summary>
//    /// Implementación del servicio de batalla.
//    /// Esta es una versión básica; reemplazar con llamadas a API real según sea necesario.
//    /// </summary>
//    public class BattleService : IBattleService
//    {
//        private readonly ApiClient _apiClient;

//        public BattleService(ApiClient apiClient)
//        {
//            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
//        }

//        public async Task<bool> SendChallengeAsync(string currentUserId, string targetUserId)
//        {
//            if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(targetUserId))
//                return false;

//            try
//            {
//                // Hacer llamada a API para enviar desafío
//                var response = await _apiClient.PostAsync("/api/battles/challenge", new
//                {
//                    challengerId = currentUserId,
//                    targetId = targetUserId
//                });

//                return response?.IsSuccessStatusCode ?? false;
//            }
//            catch (Exception ex)
//            {
//                System.Windows.MessageBox.Show($"Error al enviar desafío: {ex.Message}");
//                return false;
//            }
//        }

//        public async Task<bool> RequestJoinAsync(string currentUserId, string battleId)
//        {
//            if (string.IsNullOrWhiteSpace(currentUserId) || string.IsNullOrWhiteSpace(battleId))
//                return false;

//            try
//            {
//                // Hacer llamada a API para unirse a batalla
//                var response = await _apiClient.PostAsync($"/api/battles/{battleId}/join", new
//                {
//                    userId = currentUserId
//                });

//                return response?.IsSuccessStatusCode ?? false;
//            }
//            catch (Exception ex)
//            {
//                System.Windows.MessageBox.Show($"Error al unirse a la batalla: {ex.Message}");
//                return false;
//            }
//        }

//        public async Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId)
//        {
//            if (string.IsNullOrWhiteSpace(battleIdOrUserId))
//                return false;

//            try
//            {
//                // Esperar a que se acepte (con timeout)
//                for (int i = 0; i < 30; i++) // 30 segundos máximo
//                {
//                    var response = await _apiClient.GetAsync($"/api/battles/status/{battleIdOrUserId}");

//                    if (response?.IsSuccessStatusCode == true)
//                    {
//                        // Aquí se podría parsear la respuesta para verificar estado
//                        // Si la batalla está "active", significa que fue aceptada
//                        return true;
//                    }

//                    await Task.Delay(1000); // Esperar 1 segundo antes de reintentar
//                }

//                System.Windows.MessageBox.Show("Tiempo de espera agotado para la aceptación de la batalla.");
//                return false;
//            }
//            catch (Exception ex)
//            {
//                System.Windows.MessageBox.Show($"Error al esperar aceptación: {ex.Message}");
//                return false;
//            }
//        }

//        public async Task<BattleData> GetBattleDataAsync(string battleId)
//        {
//            if (string.IsNullOrWhiteSpace(battleId))
//                return null;

//            try
//            {
//                var response = await _apiClient.GetAsync($"/api/battles/{battleId}");

//                if (response?.IsSuccessStatusCode == true)
//                {
//                    // Aquí iría la deserialización real de la respuesta JSON
//                    // Por ahora retornamos null como placeholder
//                    // var battleData = JsonConvert.DeserializeObject<BattleData>(await response.Content.ReadAsStringAsync());
//                    // return battleData;
//                    return null;
//                }

//                return null;
//            }
//            catch (Exception ex)
//            {
//                System.Windows.MessageBox.Show($"Error al obtener datos de batalla: {ex.Message}");
//                return null;
//            }
//        }

//        public async Task<bool> CancelBattleAsync(string battleId)
//        {
//            if (string.IsNullOrWhiteSpace(battleId))
//                return false;

//            try
//            {
//                var response = await _apiClient.DeleteAsync($"/api/battles/{battleId}");
//                return response?.IsSuccessStatusCode ?? false;
//            }
//            catch (Exception ex)
//            {
//                System.Windows.MessageBox.Show($"Error al cancelar batalla: {ex.Message}");
//                return false;
//            }
//        }
//    }
//}


using System;
using System.Threading.Tasks;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class BattleService : IBattleService
    {
        public async Task<bool> SendChallengeAsync(string currentUserId, string targetUserId)
        {
            try
            {
                var result = await ApiClient.PostAsync<object>(
                    "/api/battles/challenge",
                    new { challengerId = currentUserId, targetId = targetUserId }
                );

                return result != null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al enviar desafío: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestJoinAsync(string currentUserId, string battleId)
        {
            try
            {
                var result = await ApiClient.PostAsync<object>(
                    $"/api/battles/{battleId}/join",
                    new { userId = currentUserId }
                );

                return result != null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al unirse a la batalla: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId)
        {
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    var result = await ApiClient.GetAsync<object>(
                        $"/api/battles/status/{battleIdOrUserId}"
                    );

                    if (result != null)
                        return true;

                    await Task.Delay(1000);
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al esperar aceptación: {ex.Message}");
                return false;
            }
        }

        public async Task<BattleData> GetBattleDataAsync(string battleId)
        {
            try
            {
                return await ApiClient.GetAsync<BattleData>(
                    $"/api/battles/{battleId}"
                );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al obtener datos de batalla: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CancelBattleAsync(string battleId)
        {
            try
            {
                await ApiClient.DeleteAsync($"/api/battles/{battleId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al cancelar batalla: {ex.Message}");
                return false;
            }
        }
    }
}
