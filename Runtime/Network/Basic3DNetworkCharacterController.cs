using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SinbadStudios.SharedSystems.Runtime
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public class Basic3DNetworkCharacterController : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5f;

        private NetworkCharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<NetworkCharacterController>();
            ApplyMovementSettings();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _characterController.Move(Vector3.zero);
                return;
            }

            Vector3 moveDirection = new Vector3(
                GetAxis(keyboard.aKey.isPressed, keyboard.dKey.isPressed),
                0f,
                GetAxis(keyboard.sKey.isPressed, keyboard.wKey.isPressed));

            _characterController.Move(moveDirection);
        }

        private void ApplyMovementSettings()
        {
            _characterController.maxSpeed = moveSpeed;
        }

        private static float GetAxis(bool negativePressed, bool positivePressed)
        {
            float axis = 0f;

            if (negativePressed)
            {
                axis -= 1f;
            }

            if (positivePressed)
            {
                axis += 1f;
            }

            return axis;
        }
    }
}