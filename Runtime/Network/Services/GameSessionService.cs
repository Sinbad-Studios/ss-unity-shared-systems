using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class GameSessionService
    {
        private readonly string _authToken;
        private readonly string _clientId;

        public GameSessionService(string token, string clientId)
        {
            _authToken = token;
            _clientId = clientId;
        }

        // ============================================================
        // GAME SESSION STATUS
        // ============================================================
        public async Task<GameSessionStatusDataWrapper> GetGameSessionStatusSnapshotAsync(string gameSessionId)
        {
            string endpoint = $"/games/sessions/status?session_id={gameSessionId}";
            string json = await APIManager.Instance.GetAsync(endpoint, _authToken, _clientId);

            GameSessionStatusResponse response = JsonUtility.FromJson<GameSessionStatusResponse>(json);
            if (response == null || response.data == null || response.data.session == null)
                throw new Exception("Invalid session status data.");

            return response.data;
        }

        public async Task<GameSessionStatusData> GetGameSessionStatusAsync(string gameSessionId)
        {
            GameSessionStatusDataWrapper snapshot = await GetGameSessionStatusSnapshotAsync(gameSessionId);
            return snapshot.session;
        }

        // ============================================================
        // UPDATE SESSION STATUS
        // ============================================================
        public async Task<GameSessionStatusData> UpdateUserSessionAsync(
             string userId, string sessionId, SessionUserEdgeStatus status, string winnerUserId = null, string roundResult = null)
        {
            return await UpdateUserSessionAsync(userId, sessionId, status.ToString(), winnerUserId, roundResult);
        }

        public async Task<GameSessionStatusData> UpdateUserSessionAsync(
             string userId, string sessionId, string status, string winnerUserId = null, string roundResult = null)
        {
            string endpoint = "/games/user/sessions/updater";

            bool isDrawReport = IsRoundResult(roundResult, RoundResultStatus.DRAW);
            bool isWinReport = IsRoundResult(roundResult, RoundResultStatus.WIN);

            if (isDrawReport && !string.IsNullOrEmpty(winnerUserId))
                throw new Exception("round_result DRAW must not include winner_user_id.");

            if (isWinReport && string.IsNullOrEmpty(winnerUserId))
                throw new Exception("round_result WIN requires winner_user_id.");

            if (!string.IsNullOrWhiteSpace(roundResult) && !isDrawReport && !isWinReport)
                throw new Exception($"Unsupported round_result: {roundResult}");

            object body;
            if (isDrawReport)
            {
                body = new UserSessionDrawUpdateRequest
                {
                    requestor_user_id = userId,
                    session_id = sessionId,
                    player_status = status,
                    round_result = RoundResultStatus.DRAW.ToString()
                };
            }
            else if (!string.IsNullOrEmpty(winnerUserId))
            {
                body = new UserSessionUpdateRequest
                {
                    requestor_user_id = userId,
                    session_id = sessionId,
                    player_status = status,
                    round_result = RoundResultStatus.WIN.ToString(),
                    winner_user_id = winnerUserId
                };
            }
            else
            {
                body = new UserSessionUpdateBaseRequest
                {
                    requestor_user_id = userId,
                    session_id = sessionId,
                    player_status = status
                };
            }

            string json = await APIManager.Instance.PutAsync(endpoint, body, _authToken, _clientId);

            GameSessionStatusResponse response = JsonUtility.FromJson<GameSessionStatusResponse>(json);
            if (response == null || response.data == null || response.data.session == null)
                throw new Exception("Invalid updater response.");

            return response.data.session;
        }

        // ============================================================
        // REMATCH VOTE
        // ============================================================
        public async Task<RematchVoteData> RematchVoteAsync(
            string userId, string sessionId, bool rematchAgree)
        {
            string endpoint = "/games/user/sessions/rematch-vote";

            var body = new RematchVoteRequest
            {
                requestor_user_id = userId,
                session_id = sessionId,
                rematch_agree = rematchAgree
            };

            string json = await APIManager.Instance.PatchAsync(endpoint, body, _authToken, _clientId);

            RematchVoteResponse response = JsonUtility.FromJson<RematchVoteResponse>(json);
            if (response != null && response.data != null && !string.IsNullOrEmpty(response.data.sessionId))
                return response.data;

            GameSessionStatusResponse legacyResponse = JsonUtility.FromJson<GameSessionStatusResponse>(json);
            if (legacyResponse != null && legacyResponse.data != null && legacyResponse.data.session != null)
            {
                return new RematchVoteData
                {
                    sessionId = legacyResponse.data.session.sessionId,
                    userId = userId,
                    decision = rematchAgree
                };
            }

            throw new Exception("Invalid rematch vote response.");
        }

        // ============================================================
        // HEARTBEAT
        // ============================================================
        public async Task<SessionHeartbeatData> SendSessionHeartbeatAsync(
            string sessionId, string userId)
        {
            string endpoint = "/games/user/sessions/heartbeat";

            var body = new SessionHeartbeatRequest
            {
                requestor_user_id = userId,
                session_id = sessionId
            };

            string json = await APIManager.Instance.PutAsync(endpoint, body, _authToken, _clientId);

            SessionHeartbeatResponse response = JsonUtility.FromJson<SessionHeartbeatResponse>(json);
            if (response != null && response.data != null && response.data.sessionEdge != null)
                return response.data;

            SessionHeartbeatData rawResponse = JsonUtility.FromJson<SessionHeartbeatData>(json);
            if (rawResponse != null && rawResponse.sessionEdge != null)
                return rawResponse;

            throw new Exception("Invalid heartbeat response.");
        }

        // ============================================================
        // PENDING STATUS POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForPendingStatus(string sessionId, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for PENDING.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.PENDING))
                {
                    Debug.Log($"Session {sessionId} status confirmed: {_sessionData.status}");
                    return (true, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.IN_PROGRESS) ||
                    HasSessionStatus(_sessionData, SessionStatus.BREAK) ||
                    HasSessionStatus(_sessionData, SessionStatus.COMPLETED))
                {
                    Debug.LogWarning($"Session {sessionId} moved past PENDING while waiting. Current status: {_sessionData.status}");
                    return (false, _sessionData);
                }

                Debug.Log($"Waiting for session {sessionId} to be PENDING. Current status: {_sessionData?.status ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        // ============================================================
        // IN_PROGRESS STATUS POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForInProgressStatus(string sessionId, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for IN_PROGRESS.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.IN_PROGRESS))
                {
                    Debug.Log($"Session {sessionId} status confirmed: {_sessionData.status}");
                    return (true, _sessionData);
                }

                Debug.Log($"Waiting for session {sessionId} to be IN_PROGRESS. Current status: {_sessionData?.status ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        // ============================================================
        // REMATCH IN_PROGRESS STATUS POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForRematchInProgressStatus(string sessionId, int previousRoundNumber, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);
                RoundData latestRound = GetLatestRound(_sessionData);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for rematch IN_PROGRESS.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.IN_PROGRESS) &&
                    HasRoundStatus(latestRound, RoundStatus.IN_PROGRESS) &&
                    latestRound.roundNumber > previousRoundNumber)
                {
                    Debug.Log($"Session {sessionId} rematch confirmed: {_sessionData.status}, round {latestRound.roundNumber}.");
                    return (true, _sessionData);
                }

                Debug.Log($"Waiting for rematch session {sessionId} to be IN_PROGRESS with a new active round. Current session: {_sessionData?.status ?? "null"}, round: {latestRound?.roundNumber.ToString() ?? "null"}, round status: {latestRound?.status ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        // ============================================================
        // LEGACY PENDING/FULL STATUS POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForPendingFullStatus(string sessionId, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for FULL or PENDING.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.FULL) ||
                    HasSessionStatus(_sessionData, SessionStatus.PENDING))
                {
                    Debug.Log($"Session {sessionId} status confirmed: {_sessionData.status}");
                    return (true, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.IN_PROGRESS))
                {
                    Debug.Log($"Session {sessionId} is already IN_PROGRESS. Current status: {_sessionData.status}");
                    return (false, _sessionData);
                }

                Debug.Log($"Waiting for session {sessionId} to be FULL or PENDING. Current status: {_sessionData?.status ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        // ============================================================
        // LATEST ROUND COMPLETION POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForLatestRoundCompletionAsync(string sessionId, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);
                RoundData latestRound = GetLatestRound(_sessionData);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for latest round completion.");
                    return (false, _sessionData);
                }

                if (HasRoundStatus(latestRound, RoundStatus.INTERRUPTED))
                {
                    Debug.LogWarning($"Latest round {latestRound.roundNumber} interrupted while waiting for completion.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.BREAK) &&
                    IsLatestRoundResolved(latestRound))
                {
                    Debug.Log($"Session {sessionId} entered BREAK with resolved latest round {latestRound.roundNumber}. Result: {GetRoundResolutionLabel(latestRound)}.");
                    return (true, _sessionData);
                }

                Debug.Log($"Waiting for BREAK plus resolved latest round. Current session: {_sessionData?.status ?? "null"}, latest round: {latestRound?.roundNumber.ToString() ?? "null"}, round status: {latestRound?.status ?? "null"}, round result: {latestRound?.roundResult ?? "null"}, winner: {latestRound?.winnerUserId ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        // ============================================================
        // COMPLETED STATUS POLLING
        // ============================================================
        public async Task<(bool, GameSessionStatusData)> WaitForCompletedStatus(string sessionId, float timeoutSeconds = 60f, float pollIntervalSeconds = 0.5f)
        {
            float elapsed = 0f;
            GameSessionStatusData _sessionData = null;

            while (elapsed < timeoutSeconds)
            {
                _sessionData = await GetGameSessionStatusAsync(sessionId);

                if (IsDisconnectedSession(_sessionData))
                {
                    Debug.LogWarning($"Session {sessionId} disconnected while waiting for COMPLETED.");
                    return (false, _sessionData);
                }

                if (HasSessionStatus(_sessionData, SessionStatus.COMPLETED))
                {
                    Debug.Log($"Session {sessionId} status confirmed: {_sessionData.status}");
                    return (true, _sessionData);
                }

                Debug.Log($"Waiting for session {sessionId} to be COMPLETED. Current status: {_sessionData?.status ?? "null"}. Elapsed time: {elapsed:F1}s");

                await Awaitable.WaitForSecondsAsync(pollIntervalSeconds);
                elapsed += pollIntervalSeconds;
            }

            return (false, _sessionData);
        }

        public static RoundData GetLatestRound(GameSessionStatusData sessionData)
        {
            if (sessionData?.rounds == null || sessionData.rounds.Length == 0)
            {
                return null;
            }

            return sessionData.rounds
                .Where(round => round != null)
                .OrderByDescending(round => round.roundNumber)
                .ThenByDescending(round => round.updatedAt)
                .ThenByDescending(round => round.createdAt)
                .FirstOrDefault();
        }

        private static bool IsDisconnectedSession(GameSessionStatusData sessionData)
        {
            return HasSessionStatus(sessionData, SessionStatus.DISCONNECTED);
        }

        private static bool HasSessionStatus(GameSessionStatusData sessionData, SessionStatus status)
        {
            return sessionData?.status == status.ToString();
        }

        public static bool HasRoundStatus(RoundData roundData, RoundStatus status)
        {
            return roundData?.status == status.ToString();
        }

        public static bool IsLatestRoundResolved(RoundData roundData)
        {
            if (!HasRoundStatus(roundData, RoundStatus.COMPLETED))
                return false;

            return HasWinner(roundData) || IsRoundResult(roundData?.roundResult, RoundResultStatus.DRAW);
        }

        private static bool HasWinner(RoundData roundData)
        {
            return !string.IsNullOrWhiteSpace(roundData?.winnerUserId);
        }

        private static bool IsRoundResult(string value, RoundResultStatus result)
        {
            return string.Equals(value, result.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRoundResolutionLabel(RoundData roundData)
        {
            if (IsRoundResult(roundData?.roundResult, RoundResultStatus.DRAW))
                return RoundResultStatus.DRAW.ToString();

            if (HasWinner(roundData))
            {
                string result = string.IsNullOrWhiteSpace(roundData.roundResult)
                    ? "WIN legacy"
                    : roundData.roundResult;
                return $"{result}, winner: {roundData.winnerUserId}";
            }

            return "unresolved";
        }
    }
}
