using System;

namespace SinbadStudios.SharedSystems.Runtime
{
    [Serializable]
    public class GameUserReponse
    {
        public string status;
        public GameUserData data;
    }

    [Serializable]
    public class GameUserData
    {
        public string first_name;
        public string last_name;
        public string username;
        public string photo_url;
        public string userId;
        public string token;
    }
}
