using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class UserService
    {
        private readonly string _authToken;
        private readonly string _clientId;

        public UserService(string token, string clientId)
        {
            _authToken = token;
            _clientId = clientId;
        }

        public async Task<GameUserData> GetUserInfoAsync(string userId)
        {
            string endpoint = $"/user/info?user_id={userId}";
            string json = await APIManager.Instance.GetAsync(endpoint, _authToken, _clientId);

            GameUserReponse response = JsonUtility.FromJson<GameUserReponse>(json);
            if (response == null || response.data == null)
                throw new Exception("Invalid user data.");

            return response.data;
        }
    }
}
