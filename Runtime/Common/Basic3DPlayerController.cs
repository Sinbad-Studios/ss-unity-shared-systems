using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SinbadStudios.SharedSystems.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class Basic3DPlayerController : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed = 5f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
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
                SetHorizontalVelocity(Vector3.zero);
                return;
            }

            Vector3 input = new Vector3(
                GetAxis(keyboard.aKey.isPressed, keyboard.dKey.isPressed),
                0f,
                GetAxis(keyboard.sKey.isPressed, keyboard.wKey.isPressed));

            Vector3 horizontalVelocity = input.normalized * moveSpeed;
            SetHorizontalVelocity(horizontalVelocity);
        }

        private void SetHorizontalVelocity(Vector3 horizontalVelocity)
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;

            _rigidbody.linearVelocity = new Vector3(
                horizontalVelocity.x,
                currentVelocity.y,
                horizontalVelocity.z);
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
