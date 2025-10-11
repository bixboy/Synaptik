using NaughtyAttributes;
using Synaptik.Core;
using UnityEngine;

namespace Synaptik.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody rigidbodyComponent;

        [Header("Movement (plan XZ)")]
        [SerializeField, Min(0f)]
        private float maxSpeed = 5f;

        [SerializeField, Min(0f)]
        private float acceleration = 20f;

        [SerializeField, Min(0f)]
        private float deceleration = 25f;

        [Header("Camera-relative ?")]
        [SerializeField]
        private bool cameraRelative = true;

        [SerializeField, ShowIf(nameof(cameraRelative))]
        private Camera targetCamera;

        private void Reset()
        {
            rigidbodyComponent = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            if (rigidbodyComponent == null)
            {
                rigidbodyComponent = GetComponent<Rigidbody>();
            }

            if (cameraRelative && targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            rigidbodyComponent.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rigidbodyComponent.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            var input = InputsDetection.Instance != null ? InputsDetection.Instance.MoveVector : Vector2.zero;
            var direction = GetMovementDirection(input);

            var currentVelocity = rigidbodyComponent.velocity;
            var horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            var targetHorizontalVelocity = direction * maxSpeed;

            var currentAcceleration = direction.sqrMagnitude > 0.0001f ? acceleration : deceleration;
            var maxDeltaV = currentAcceleration * Time.fixedDeltaTime;
            var deltaV = targetHorizontalVelocity - horizontalVelocity;

            if (deltaV.sqrMagnitude > maxDeltaV * maxDeltaV)
            {
                deltaV = deltaV.normalized * maxDeltaV;
            }

            rigidbodyComponent.AddForce(deltaV, ForceMode.VelocityChange);
        }

        private Vector3 GetMovementDirection(Vector2 input)
        {
            if (!cameraRelative || targetCamera == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            var forward = targetCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = targetCamera.transform.right;
            right.y = 0f;
            right.Normalize();

            return right * input.x + forward * input.y;
        }
    }
}
