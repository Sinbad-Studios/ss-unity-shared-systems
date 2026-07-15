using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class UserSessionUpdateBaseRequest
    {
        public string requestor_user_id;
        public string session_id;
        public string player_status;
    }
}
