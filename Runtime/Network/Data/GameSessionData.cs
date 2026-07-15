
using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class GameSessionStatusResponse
    {
        public string status;
        public GameSessionStatusDataWrapper data;
    }

    [Serializable]
    public class SessionHeartbeatResponse
    {
        public string status;
        public SessionHeartbeatData data;
    }

    [Serializable]
    public class SessionHeartbeatData
    {
        public string sessionId;
        public string userId;
        public SessionEdge sessionEdge;
    }

    [Serializable]
    public class GameSessionStatusDataWrapper
    {
        public GameSessionStatusData session;
        public PlayerData[] players;
    }

    [Serializable]
    public class GameSessionStatusData
    {
        public string sessionId;
        public string sessionType;
        public string gameId;
        public string currency;
        public string status;
        public int initialBetAmount;
        public int initialPotAmount;
        public int currentPlayers;
        public int maxPlayers;
        public string[] playerIds;
        public string requestorUserId;
        public RoundData[] rounds;
        public long createdAt;
        public long updatedAt;
    }

    [Serializable]
    public class RoundData
    {
        public string roundId;
        public string sessionId;
        public string gameId;
        public int roundNumber;
        public long startTime;
        public long endTime;
        public string winnerUserId;
        public string roundResult;
        public string[] playerIds;
        public string status;
        public int betAmount;
        public int potAmount;
        public long payoutAppliedAt;
        public long createdAt;
        public long updatedAt;
    }

    [Serializable]
    public class PlayerData
    {
        public string userId;
        public string first_name;
        public string last_name;
        public string username;
        public string photo_url;
        public SessionEdge sessionEdge;
    }

    [Serializable]
    public class SessionEdge
    {
        public string playerStatus;
        public bool rematchAgree;
        public string reportedRoundResult;
        public string reportedWinnerUserId;
        public long joinedAt;
        public long lastUpdatedAt;
        public long lastSeenAt;
    }
}
