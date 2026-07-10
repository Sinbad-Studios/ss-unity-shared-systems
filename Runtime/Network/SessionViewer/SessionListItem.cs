using Fusion;
using TMPro;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class SessionListItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI roomName;
        [SerializeField]
        private TextMeshProUGUI playerCount;

        public void UpdateSessionInfo(SessionInfo sessionInfo)
        {
            roomName.text = sessionInfo.Name;
            playerCount.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";
        }
    }
}