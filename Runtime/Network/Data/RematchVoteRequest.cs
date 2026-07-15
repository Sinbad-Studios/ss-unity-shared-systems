using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class RematchVoteRequest
    {
        public string requestor_user_id;
        public string session_id;
        public bool rematch_agree;
    }

    [Serializable]
    public class RematchVoteResponse
    {
        public string status;
        public RematchVoteData data;
    }

    [Serializable]
    public class RematchVoteData
    {
        public string sessionId;
        public string userId;
        public bool decision;
    }
}
