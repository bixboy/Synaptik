using UnityEngine;

namespace Synaptik.Gameplay
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Cible à suivre")]
        [SerializeField]
        private Transform player;

        [Header("Paramètres de vue")]
        [SerializeField]
        private Vector3 offset = new(0f, 10f, -8f);

        [SerializeField]
        private float rotationSmoothness = 5f;

        [Header("Limites de rotation")]
        [SerializeField]
        private float maxYawAngle = 45f;

        [SerializeField]
        private float maxPitchAngle = 20f;

        [Header("Zoom")]
        [SerializeField]
        private float zoomSpeed = 5f;

        [SerializeField]
        private float minZoom = 5f;

        [SerializeField]
        private float maxZoom = 15f;

        private float currentZoom;
        private float targetZoom;
        private Quaternion baseRotation;

        private void Start()
        {
            if (player == null)
            {
                Debug.LogWarning("[CameraFollow] Aucun player assigné à la caméra.");
                enabled = false;
                return;
            }

            currentZoom = offset.magnitude;
            targetZoom = currentZoom;
            baseRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
            if (TryGetComponent<Camera>(out var camera))
            {
                var baseFov = 60f - (currentZoom - minZoom) * 2f;
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, baseFov, Time.deltaTime * zoomSpeed);
            }

            var dirToPlayer = player.position - transform.position;
            var targetRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            var smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
            transform.rotation = LimitRotation(smoothRotation, baseRotation, maxYawAngle, maxPitchAngle);
        }

        public void ZoomIn(float amount = 2f)
        {
            targetZoom = Mathf.Max(minZoom, targetZoom - amount);
        }

        public void ZoomOut(float amount = 2f)
        {
            targetZoom = Mathf.Min(maxZoom, targetZoom + amount);
        }

        private static Quaternion LimitRotation(Quaternion current, Quaternion reference, float maxYaw, float maxPitch)
        {
            var currentEuler = current.eulerAngles;
            var baseEuler = reference.eulerAngles;

            var yawDelta = Mathf.Clamp(Mathf.DeltaAngle(baseEuler.y, currentEuler.y), -maxYaw, maxYaw);
            var pitchDelta = Mathf.Clamp(Mathf.DeltaAngle(baseEuler.x, currentEuler.x), -maxPitch, maxPitch);

            return Quaternion.Euler(baseEuler.x + pitchDelta, baseEuler.y + yawDelta, 0f);
        }
    }
}
