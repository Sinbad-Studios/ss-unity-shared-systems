using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class FusionSessionViewer : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private string lobbyName = "SinbadGamesLobby";
        [SerializeField] private Transform sessionListContentParent;
        [SerializeField] private GameObject sessionListEntryPrefab;

        public static NetworkRunner runnerInstance;
        public Dictionary<string, GameObject> sessionListUIDictionary = new Dictionary<string, GameObject>();

        private void Awake()
        {
            runnerInstance = gameObject.GetComponent<NetworkRunner>();

            if (runnerInstance == null)
            {
                runnerInstance = gameObject.AddComponent<NetworkRunner>();
            }
        }

        private void Start()
        {
            runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            DeleteOldSessionsFromUI(sessionList);
            CompareLists(sessionList);
        }

        private void CompareLists(List<SessionInfo> sessionList)
        {
            foreach (SessionInfo session in sessionList)
            {
                if (sessionListUIDictionary.ContainsKey(session.Name))
                {
                    UpdateEntryUI(session);
                }
                else
                {
                    Debug.Log($"Creating new session UI entry: {session.Name}");
                    CreateEntryUI(session);
                }
            }
        }

        private void UpdateEntryUI(SessionInfo session)
        {
            sessionListUIDictionary.TryGetValue(session.Name, out GameObject existingEntry);
            SessionListItem entryScript = existingEntry.GetComponent<SessionListItem>();
            entryScript.UpdateSessionInfo(session);

            existingEntry.SetActive(session.IsVisible);
        }

        private void CreateEntryUI(SessionInfo session)
        {
            GameObject newEntry = Instantiate(sessionListEntryPrefab, sessionListContentParent);
            SessionListItem entryScript = newEntry.GetComponent<SessionListItem>();
            sessionListUIDictionary.Add(session.Name, newEntry);
            entryScript.UpdateSessionInfo(session);

            newEntry.SetActive(session.IsVisible);
        }
        private void DeleteOldSessionsFromUI(List<SessionInfo> sessionList)
        {
            List<string> keysToRemove = new List<string>();

            foreach (KeyValuePair<string, GameObject> kvp in sessionListUIDictionary)
            {
                string sessionKey = kvp.Key;
                bool isContained = false;

                foreach (SessionInfo sessionInfo in sessionList)
                {
                    if (sessionInfo.Name == sessionKey)
                    {
                        isContained = true;
                        break;
                    }
                }

                if (!isContained)
                {
                    Debug.Log($"Removing old session UI: {sessionKey}");
                    Destroy(kvp.Value);
                    keysToRemove.Add(sessionKey);
                }
            }

            foreach (string key in keysToRemove)
            {
                sessionListUIDictionary.Remove(key);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}
