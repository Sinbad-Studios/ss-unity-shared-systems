using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class NetworkManager : MonoSingleton<NetworkManager>, INetworkRunnerCallbacks
    {
        [SerializeField, ReadOnly] private string joinedSessionName;
        [SerializeField, ReadOnly] private bool hasSessionStarted = false;

        [Header("Network Settings")]
        [SerializeField] private NetworkRunner networkRunnerPrefab;
        [Tooltip("Leave game scene empty if you want to use the default scene for the game.")]
        [SerializeField, ScenePath] private string gameScene;
        [SerializeField] private string lobbyName = "SinbadGamesLobby";
        [SerializeField] private string sessionGameName = "sessionGameName";
        [SerializeField] private string dedicatedSessionName = "";
        [SerializeField] private int maxPlayers = 2;

        [Header("Reconnection Settings")]
        [Tooltip("Delay in seconds before retrying to join the lobby")]
        [SerializeField] private float _retryDelay = 5f;
        [Tooltip("Maximum number of reconnection attempts")]
        [SerializeField] private int _maxRetries = 3;

        private bool _useDedicated = false;
        private int _retryCount = 0;
        private bool _isInLobby = false;
        private NetworkRunner _runnerInstance;
        private INetworkSceneManager _networkSceneManager;

        protected override void Init()
        {
            GameEventBus.Instance.Subscribe<LeaveSessionEvent>(OnLeaveSession);
            GameEventBus.Instance.Subscribe<JoinDirectSessionEvent>(OnJoinDirectSession);
            GameEventBus.Instance.Subscribe<JoinLobbyEvent>(OnJoinLobby);
        }

        private async void OnLeaveSession(LeaveSessionEvent eventData)
        {
            try
            {
                await LeaveSession();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Leave session failed: {ex}");
            }
        }

        private async void OnJoinDirectSession(JoinDirectSessionEvent eventData)
        {
            try
            {
                await DirectJoinOrCreateSession(eventData.SessionName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Leave session failed: {ex}");
            }
        }

        private async void OnJoinLobby(JoinLobbyEvent eventData)
        {
            try
            {
                await JoinLobby();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Join lobby failed: {ex}");
            }
        }

        private async Task DirectJoinOrCreateSession(string sessionName)
        {
            PublishStatus("Joining session...");

            if (_runnerInstance)
            {
                await _runnerInstance.Shutdown();
            }

            _runnerInstance = InstantiateRunner("GameRunner");

            _useDedicated = true;

            if (string.IsNullOrEmpty(sessionName))
            {
                PublishStatus("Invalid Session\n<size=50><color=#FF0000>Connecting to Lobby...");
                await Awaitable.WaitForSecondsAsync(3f);
                Debug.LogError("Session name is null or empty.");
                await JoinLobby();
                return;
            }

            var args = new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                IsOpen = true,
                IsVisible = false,
                SceneManager = _networkSceneManager
            };

            var result = await _runnerInstance.StartGame(args);
            if (result.Ok)
            {
                PublishStatus("Waiting for opponent...");
                Debug.Log($"Successfully joined or created session: {sessionName}!");
                joinedSessionName = sessionName;
                GameEventBus.Instance.Publish(new SessionFoundEvent());
                if (_runnerInstance.IsSharedModeMasterClient && !string.IsNullOrEmpty(gameScene))
                {
                    await _runnerInstance.LoadScene(gameScene, LoadSceneMode.Single);
                }
            }
            else
            {
                PublishStatus($"Failed to join session\n<size=50><color=#FF0000>{result.ShutdownReason}...");
                Debug.LogError($"Failed to join or create: {result.ShutdownReason}");
                GameEventBus.Instance.Publish(new SessionConnectFailedEvent());
            }
        }

        private async Task JoinLobby()
        {
            if (_runnerInstance)
            {
                await _runnerInstance.Shutdown();
            }

            _runnerInstance = InstantiateRunner("GameRunner");

            PublishStatus("Connecting to lobby...");
            var result = await _runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
            if (result.Ok)
            {
                PublishStatus("Searching for opponents...");
                Debug.Log("Successfully joined lobby. Waiting for session list updates...");
                _isInLobby = true;
            }
            else
            {
                PublishStatus("Failed to connect to lobby. Retrying...");
                Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
                _runnerInstance = null;
                GameEventBus.Instance.Publish(new LobbyFailedToConnectEvent());
            }
        }

        private NetworkRunner InstantiateRunner(string runnerId)
        {
            var runner = Instantiate(networkRunnerPrefab);
            runner.name = runnerId;
            runner.AddCallbacks(this);

            if (runner.TryGetComponent<INetworkSceneManager>(out var sceneManager))
            {
                _networkSceneManager = sceneManager;
            }
            else
            {
                Debug.LogError("NetworkRunner does not have a component that implements INetworkSceneManager.");
            }

            return runner;
        }

        private async Task LeaveSession()
        {
            var playerObj = _runnerInstance.GetPlayerObject(_runnerInstance.LocalPlayer);
            if (playerObj != null)
            {
                _runnerInstance.Despawn(playerObj);
            }
            await _runnerInstance.Shutdown(true, ShutdownReason.Ok);
            joinedSessionName = null;
            Destroy(_runnerInstance.gameObject);
        }

        private void ResetState()
        {
            _isInLobby = false;
            hasSessionStarted = false;
            _retryCount = 0;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} joined the session.");
            GameEventBus.Instance.Publish(new PlayerJoinedEvent { Runner = runner, Player = player });

            if (!hasSessionStarted && runner.ActivePlayers.Count() >= runner.SessionInfo.MaxPlayers)
            {
                StartGameSession(runner);
            }
        }

        public void StartGameSession(NetworkRunner runner) //Called when the session is full and ready to start or when the host decides to start the game
        {
            PublishStatus("<color=#FFE066>Starting\nGame</color>", false);
            Debug.Log("Session is full. Starting game...");
            runner.SessionInfo.IsVisible = false;
            GameEventBus.Instance.Publish(new SessionReadyToStartEvent());
            hasSessionStarted = true;
        }

        public async void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (!_isInLobby)
            {
                return;
            }

            PublishStatus("Scanning for open sessions...");

            SessionInfo targetSession = null;
            foreach (SessionInfo session in sessionList)
            {
                if (session.Name.Contains(sessionGameName))
                {
                    if (session.PlayerCount == 1 && session.IsOpen && session.IsValid)
                    {
                        targetSession = session;
                        break;
                    }
                }
            }

            if (targetSession != null)
            {
                Debug.Log($"Found open 1v1 session: {targetSession.Name}. Joining...");
                await DirectJoinOrCreateSession(targetSession.Name);
            }
            else
            {
                Debug.Log("No open 1v1 sessions found. Creating a new one...");
                string newSessionName = $"{sessionGameName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                await DirectJoinOrCreateSession(newSessionName);
            }
        }

        private async Task DoRetryConnectionAfterDelay()
        {
            PublishStatus($"Retrying connection in {_retryDelay} seconds...");
            await Awaitable.WaitForSecondsAsync(_retryDelay);
            _retryCount++;
            if (_useDedicated)
            {
                await DirectJoinOrCreateSession(dedicatedSessionName);
            }
            else
            {
                await JoinLobby();
            }
        }

        public async void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogWarning($"Connection to {remoteAddress} failed: {reason}");
            if (_retryCount < _maxRetries)
            {
                Debug.Log($"Retrying... Attempt {_retryCount + 1} of {_maxRetries}");
                GameEventBus.Instance.Publish(new LobbyReconnectAttemptEvent()); // Or rename event if not lobby-specific
                await DoRetryConnectionAfterDelay();
            }
            else
            {
                PublishStatus("Connection failed. Max retries reached.");
                Debug.LogError("Max reconnection attempts reached.");
                ResetState();
                GameEventBus.Instance.Publish(new LobbyFailedToConnectEvent());
            }
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReason == ShutdownReason.Ok)
            {
                Debug.Log("Clean shutdown completed.");
                ResetState();
                GameEventBus.Instance.Unsubscribe<LeaveSessionEvent>(OnLeaveSession);
                GameEventBus.Instance.Unsubscribe<JoinDirectSessionEvent>(OnJoinDirectSession);
                GameEventBus.Instance.Unsubscribe<JoinLobbyEvent>(OnJoinLobby);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            PublishStatus("Opponent left. Waiting for reconnect...");
            Debug.Log($"Player {player} left the session.");
            NetworkObject playerObject = runner.GetPlayerObject(player);
            GameEventBus.Instance.Publish(new PlayerLeftEvent
            {
                Runner = runner,
                Player = player,
                PlayerObject = playerObject
            });
        }

        private void OnDestroy()
        {
            if (_runnerInstance != null)
            {
                _runnerInstance.RemoveCallbacks(this);
            }

            GameEventBus.Instance.Unsubscribe<LeaveSessionEvent>(OnLeaveSession);
            GameEventBus.Instance.Unsubscribe<JoinDirectSessionEvent>(OnJoinDirectSession);
            GameEventBus.Instance.Unsubscribe<JoinLobbyEvent>(OnJoinLobby);
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        private void PublishStatus(string message, bool messageEffect = true)
        {
            GameEventBus.Instance.Publish(new NetworkStatusUpdateEvent { Message = message, MessageEffect = messageEffect });
        }
    }
}
