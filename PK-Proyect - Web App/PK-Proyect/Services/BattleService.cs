using System;
using System.Threading.Tasks;
using PK_Proyect.Repositories;

namespace PK_Proyect.Services
{
    public class BattleService : IBattleService
    {
        // POST /battles/challenge  {target_id: ...}
        public async Task<bool> SendChallengeAsync(string currentUserId, string targetUserId)
        {
            try
            {
                var result = await ApiClient.PostAsync<object>(
                    "/battles/challenge",
                    new { target_id = targetUserId }
                );
                return result != null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al enviar desafio: {ex.Message}");
                return false;
            }
        }

        // POST /battles/{battleId}/join
        public async Task<bool> RequestJoinAsync(string currentUserId, string battleId)
        {
            try
            {
                var result = await ApiClient.PostAsync<object>(
                    $"/battles/{battleId}/join",
                    new { }
                );
                return result != null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al unirse a la batalla: {ex.Message}");
                return false;
            }
        }

        // Polling GET /battles/{battleIdOrUserId}/status  hasta que status == "active" o timeout
        public async Task<bool> WaitForAcceptanceAsync(string battleIdOrUserId)
        {
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    var result = await ApiClient.GetAsync<BattleStatusResponse>(
                        $"/battles/{battleIdOrUserId}/status"
                    );

                    if (result != null && result.Status == "active")
                        return true;

                    await Task.Delay(1000);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al esperar aceptacion: {ex.Message}");
                return false;
            }
        }

        // GET /battles/{battleId}
        public async Task<BattleData> GetBattleDataAsync(string battleId)
        {
            try
            {
                return await ApiClient.GetAsync<BattleData>($"/battles/{battleId}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al obtener datos de batalla: {ex.Message}");
                return null;
            }
        }

        // DELETE /battles/{battleId}
        public async Task<bool> CancelBattleAsync(string battleId)
        {
            try
            {
                await ApiClient.DeleteAsync($"/battles/{battleId}");
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al cancelar batalla: {ex.Message}");
                return false;
            }
        }
    }

    // DTO auxiliar para el polling de estado
    internal class BattleStatusResponse
    {
        public string Status { get; set; }  // "waiting", "active", "finished"
    }
}
