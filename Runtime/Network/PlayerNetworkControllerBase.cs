using Fusion;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public abstract class PlayerNetworkControllerBase : NetworkBehaviour
    {
        [Header("NETWORKED PROPERTIES")]
        [Networked] public string PlayerName { get; set; }
        [Networked] public NetworkString<_512> UserId { get; set; }
        [Networked] public NetworkBool IsDead { get; set; }
        [Networked] public NetworkBool IsGameOver { get; set; }
        [Networked] public NetworkBool PlayerDisconnected { get; set; }
        [Networked] public int MaxHealth { get; set; } = 100;
        [Networked, OnChangedRender(nameof(OnHealthChanged))]
        public int CurrentHealth { get; set; } = 100;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHealth = MaxHealth;
                PlayerDisconnected = false;
            }

            GameEventBus.Instance.Publish(new PlayerNetworkObjectSpawnedEvent
            {
                NetworkObject = Object,
                PlayerNetworkController = this
            });
        }

        protected virtual void OnHealthChanged()
        {
            if (HasStateAuthority)
            {
                Debug.Log($"Health changed to: {CurrentHealth}");
            }
        }

        public virtual void ResetForRematch()
        {
            IsGameOver = false;
            IsDead = false;
            CurrentHealth = MaxHealth;
            PlayerDisconnected = false;
        }
    }
}
