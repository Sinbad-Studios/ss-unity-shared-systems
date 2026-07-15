using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class UserSessionUpdateRequest
    {
        public string requestor_user_id;
        public string session_id;
        public string player_status;
        public string round_result;
        public string winner_user_id;
    }

    [Serializable]
    public class UserSessionDrawUpdateRequest
    {
        public string requestor_user_id;
        public string session_id;
        public string player_status;
        public string round_result;
    }
}
