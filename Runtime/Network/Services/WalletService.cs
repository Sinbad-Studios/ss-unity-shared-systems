using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class WalletService : MonoBehaviour
    {
        private readonly string _authToken;
        private readonly string _clientId;

        public WalletService(string token, string clientId)
        {
            _authToken = token;
            _clientId = clientId;
        }

        public async Task<string> ApplyWinningsAndLossesAsync(string sessionId)
        {
            string endpoint = "/wallet/apply-winnings-and-losses";

            var body = new ApplyWinningsRequest
            {
                session_id = sessionId
            };

            // Use APIManager's PUT method for simplicity; you could also create a PostAsync method
            string json = await APIManager.Instance.PostAsync(endpoint, body, _authToken, _clientId);

            Debug.Log("Apply Winnings Response: " + json);
            return json;
        }

        public async Task<int> CheckBalance(string userId)
        {
            string endpoint = $"/wallet/balance?requestor_user_id={userId}";
            string json = await APIManager.Instance.GetAsync(endpoint, _authToken, _clientId);

            WalletBalance response = JsonUtility.FromJson<WalletBalance>(json);

            if (response == null || response.data == null)
            {
                throw new Exception("Invalid wallet balance response.");
            }

            return response.data.balance;
        }
    }

    [Serializable]
    public class ApplyWinningsRequest
    {
        public string session_id;
    }
}
