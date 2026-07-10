using Fusion;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class NetworkStatusUpdateEvent
    {
        public string Message { get; set; }
        public bool MessageEffect { get; set; }
    }

    public class LoadSceneEvent
    {
        public string SceneName { get; set; }
    }

    public class JoinDirectSessionEvent
    {
        public string SessionName { get; set; }
    }

    public class JoinLobbyEvent { }

    public class PlayerJoinedEvent
    {
        public NetworkRunner Runner { get; set; }
        public PlayerRef Player { get; set; }
    }

    public class PlayerNetworkObjectSpawnedEvent
    {
        public NetworkObject NetworkObject { get; set; }
        public PlayerNetworkController PlayerNetworkController { get; set; }
    }

    public class PlayerLeftEvent
    {
        public NetworkRunner Runner { get; set; }
        public PlayerRef Player { get; set; }
        public NetworkObject PlayerObject { get; set; }
    }

    public class SessionReadyToStartEvent { }

    public class SessionFoundEvent { }

    public class SessionConnectFailedEvent { }

    public class LeaveSessionEvent { }

    public class LobbyReconnectAttemptEvent { }

    public class LobbyFailedToConnectEvent { }
}
