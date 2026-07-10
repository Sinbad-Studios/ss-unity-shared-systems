using Fusion;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class NetworkSceneObjectTester : NetworkBehaviour
    {
        public override void Spawned()
        {
            Debug.Log($"{name} spawned/attached. " +
                      $"NetworkId: {Object.Id}, " +
                      $"HasStateAuthority: {Object.HasStateAuthority}, " +
                      $"HasInputAuthority: {Object.HasInputAuthority}");
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{name} is running in Fusion simulation.");
            }
        }
    }
}
