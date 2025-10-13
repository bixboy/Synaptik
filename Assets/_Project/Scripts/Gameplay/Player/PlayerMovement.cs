using NaughtyAttributes;
using UnityEngine;

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

    [Header("Rotation")]
    [SerializeField, Min(0f)]
    private float rotationSpeed = 10f;

    private void Reset()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (rigidbodyComponent == null)
            rigidbodyComponent = GetComponent<Rigidbody>();

        if (cameraRelative && targetCamera == null)
            targetCamera = Camera.main;

        rigidbodyComponent.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rigidbodyComponent.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        var input = InputsDetection.Instance != null ? InputsDetection.Instance.MoveVector : Vector2.zero;
        var direction = GetMovementDirection(input);

        var currentVelocity = rigidbodyComponent.linearVelocity;
        var horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        var targetHorizontalVelocity = direction * maxSpeed;

        var currentAcceleration = direction.sqrMagnitude > 0.0001f ? acceleration : deceleration;
        var maxDeltaV = currentAcceleration * Time.fixedDeltaTime;
        var deltaV = targetHorizontalVelocity - horizontalVelocity;

        if (deltaV.sqrMagnitude > maxDeltaV * maxDeltaV)
            deltaV = deltaV.normalized * maxDeltaV;

        rigidbodyComponent.AddForce(deltaV, ForceMode.VelocityChange);

        UpdateRotation(direction, horizontalVelocity);
    }

    private Vector3 GetMovementDirection(Vector2 input)
    {
        if (!cameraRelative || !targetCamera)
            return new Vector3(input.x, 0f, input.y);

        Vector3 camForward = targetCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;

        Vector3 moveDir = (camForward * input.y + camRight * input.x);
        return moveDir.sqrMagnitude > 1f ? moveDir.normalized : moveDir;
    }

    private void UpdateRotation(Vector3 inputDir, Vector3 velocity)
    {
        Vector3 dir = inputDir.sqrMagnitude > 0.001f ? inputDir : velocity.normalized;
        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime * 100f);
    }
}
