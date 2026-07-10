using Fusion;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    public class BasicPlayerController : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5f;

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    transform.position += Vector3.up * moveSpeed * Runner.DeltaTime;
                    Debug.Log("Moving Up");
                }
                if (Input.GetKey(KeyCode.S))
                {
                    transform.position += Vector3.down * moveSpeed * Runner.DeltaTime;
                    Debug.Log("Moving Down");
                }
                if (Input.GetKey(KeyCode.A))
                {
                    transform.position += Vector3.left * moveSpeed * Runner.DeltaTime;
                    Debug.Log("Moving Left");
                }
                if (Input.GetKey(KeyCode.D))
                {
                    transform.position += Vector3.right * moveSpeed * Runner.DeltaTime;
                    Debug.Log("Moving Right");
                }
            }
        }
    }
}
