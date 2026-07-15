using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class SessionHeartbeatRequest
    {
        public string requestor_user_id;
        public string session_id;
    }
}
