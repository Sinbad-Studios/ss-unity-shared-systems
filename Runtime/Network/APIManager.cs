using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class APIManager : MonoSingleton<APIManager>
    {
        [SerializeField] private string base_URL;
        private string _storedAuthToken;
        private string _storedClientId;

        protected override void Init()
        {
            if (base_URL == null)
            {
                base_URL = "https://sandbox.api.betcat7.com/v1";
                Debug.LogWarning("APIManager: base_URL is not set. Using default sandbox URL.");
            }
        }

        [Serializable]
        private class AuthEnvelope
        {
            public AuthData _auth;
        }

        [Serializable]
        private class AuthData
        {
            public string refreshed_token;
        }

        public string CurrentAuthToken => _storedAuthToken;
        public string CurrentClientId => _storedClientId;

        public void SetAuthContext(string authToken, string clientId)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                _storedAuthToken = authToken;
                SyncSessionAuthToken(authToken);
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                _storedClientId = clientId;
            }
        }

        // ============================================================
        // BASE GET (Async)
        // ============================================================
        public async Task<string> GetAsync(string endpoint, string authToken = null, string clientId = null)
        {
            string url = BuildUrl(endpoint);
            string resolvedAuthToken = ResolveAuthToken(authToken);
            string resolvedClientId = ResolveClientId(clientId);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(resolvedAuthToken))
                    request.SetRequestHeader("Authorization", $"Bearer {resolvedAuthToken}");

                if (!string.IsNullOrEmpty(resolvedClientId))
                    request.SetRequestHeader("x-client-id", resolvedClientId);

                return await SendAsync(request, "GET", url);
            }
        }

        // ============================================================
        // BASE PUT (Async)
        // ============================================================
        public async Task<string> PutAsync(string endpoint, object bodyObject, string authToken = null, string clientId = null)
        {
            string url = BuildUrl(endpoint);
            string jsonBody = JsonUtility.ToJson(bodyObject);
            string resolvedAuthToken = ResolveAuthToken(authToken);
            string resolvedClientId = ResolveClientId(clientId);

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(resolvedAuthToken))
                    request.SetRequestHeader("Authorization", $"Bearer {resolvedAuthToken}");

                if (!string.IsNullOrEmpty(resolvedClientId))
                    request.SetRequestHeader("x-client-id", resolvedClientId);

                Debug.Log("PUT Body: " + jsonBody);
                return await SendAsync(request, "PUT", url);
            }
        }

        // ============================================================
        // BASE POST (Async)
        // ============================================================

        public async Task<string> PostAsync(string endpoint, object bodyObject, string authToken = null, string clientId = null)
        {
            string url = BuildUrl(endpoint);
            string jsonBody = JsonUtility.ToJson(bodyObject);
            string resolvedAuthToken = ResolveAuthToken(authToken);
            string resolvedClientId = ResolveClientId(clientId);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(resolvedAuthToken))
                    request.SetRequestHeader("Authorization", $"Bearer {resolvedAuthToken}");

                if (!string.IsNullOrEmpty(resolvedClientId))
                    request.SetRequestHeader("x-client-id", resolvedClientId);

                Debug.Log("POST Body: " + jsonBody);
                return await SendAsync(request, "POST", url);
            }
        }

        // ============================================================
        // BASE PATCH (Async)
        // ============================================================
        public async Task<string> PatchAsync(string endpoint, object bodyObject, string authToken = null, string clientId = null)
        {
            string url = BuildUrl(endpoint);
            string jsonBody = JsonUtility.ToJson(bodyObject);
            string resolvedAuthToken = ResolveAuthToken(authToken);
            string resolvedClientId = ResolveClientId(clientId);

            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(resolvedAuthToken))
                    request.SetRequestHeader("Authorization", $"Bearer {resolvedAuthToken}");

                if (!string.IsNullOrEmpty(resolvedClientId))
                    request.SetRequestHeader("x-client-id", resolvedClientId);

                Debug.Log("PATCH Body: " + jsonBody);
                return await SendAsync(request, "PATCH", url);
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private async Task<string> SendAsync(UnityWebRequest request, string method, string url)
        {
            Debug.Log($"{method} → {url}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            string responseBody = request.downloadHandler?.text;
            TryRefreshTokenFromResponse(responseBody);

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"{method} Success: {responseBody}");
                return responseBody;
            }

            string errorMessage = $"{method} Error ({request.responseCode}): {request.error}";
            if (!string.IsNullOrEmpty(responseBody))
            {
                errorMessage += $" | Body: {responseBody}";
            }

            Debug.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        private string ResolveAuthToken(string fallbackAuthToken)
        {
            if (!string.IsNullOrEmpty(_storedAuthToken))
            {
                return _storedAuthToken;
            }

            if (!string.IsNullOrEmpty(fallbackAuthToken))
            {
                _storedAuthToken = fallbackAuthToken;
                SyncSessionAuthToken(fallbackAuthToken);
                return fallbackAuthToken;
            }

            return null;
        }

        private string ResolveClientId(string fallbackClientId)
        {
            if (!string.IsNullOrEmpty(_storedClientId))
            {
                return _storedClientId;
            }

            if (!string.IsNullOrEmpty(fallbackClientId))
            {
                _storedClientId = fallbackClientId;
                return fallbackClientId;
            }

            return null;
        }

        private void TryRefreshTokenFromResponse(string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody))
            {
                return;
            }

            try
            {
                AuthEnvelope envelope = JsonUtility.FromJson<AuthEnvelope>(responseBody);
                string refreshedToken = envelope?._auth?.refreshed_token;

                if (string.IsNullOrEmpty(refreshedToken) || refreshedToken == _storedAuthToken)
                {
                    return;
                }

                _storedAuthToken = refreshedToken;
                SyncSessionAuthToken(refreshedToken);
                Debug.Log("JWT refreshed from API response.");
            }
            catch
            {
                // Ignore malformed or non-JSON responses.
            }
        }

        private void SyncSessionAuthToken(string authToken)
        {
            if (URLReader.SessionData != null)
            {
                URLReader.SessionData.authToken = authToken;
            }
        }

        private string BuildUrl(string endpoint)
        {
            if (endpoint.StartsWith("http"))
                return endpoint;

            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;

            return base_URL + endpoint;
        }
    }
}
