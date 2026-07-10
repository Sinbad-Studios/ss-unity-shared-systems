using System.Runtime.InteropServices;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class URLReader : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern string GetURLParameter(string paramName);

        [System.Serializable]
        public class PlayerSessionData
        {
            public string userId;
            public string sessionId;
            public string authToken;
            public string clientId;
        }

        public static PlayerSessionData SessionData { get; private set; }

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        SessionData = new PlayerSessionData
        {
            userId = GetURLParameter("user_id"),
            sessionId = GetURLParameter("session_id"),
            authToken = GetURLParameter("token"),
            clientId = GetURLParameter("client_id")
        };

        Debug.Log($"[WebGL Params] User ID: {SessionData.userId} | Session: {SessionData.sessionId} | Token: {SessionData.authToken} | Client ID: {SessionData.clientId}");
#endif
        }
    }
}
