using Fusion;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class PlayerNetworkController : NetworkBehaviour
    {
        [Header("NETWORKED PROPERTIES")]
        #region NETWORKED PROPERTIES
        [Networked] public string PlayerName { get; set; }
        [Networked] public NetworkString<_512> UserId { get; set; }
        [Networked] public NetworkBool IsDead { get; set; }
        [Networked] public NetworkBool IsGameOver { get; set; }
        [Networked] public NetworkBool PlayerDisconnected { get; set; }
        [Networked] public int MaxHealth { get; set; } = 100;
        [Networked, OnChangedRender(nameof(OnHealthChange))] public int CurrentHealth { get; set; } = 100;
        #endregion

        private void Awake()
        {

        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHealth = MaxHealth;
                PlayerDisconnected = false;
            }

            GameEventBus.Instance.Publish(new PlayerNetworkObjectSpawnedEvent { NetworkObject = Object, PlayerNetworkController = this });
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (hasState)
            {
                //Changes on player UI
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                // Handle networked physics and input here
            }
        }

        private void OnDisable()
        {

        }

        private void OnHealthChange()
        {
            if (HasStateAuthority)
            {
                Debug.Log($"Health changed to: {CurrentHealth}");
            }
        }

        public void ResetForRematch()
        {
            IsGameOver = false;
            IsDead = false;
            CurrentHealth = MaxHealth;
            PlayerDisconnected = false;
        }
    }
}
