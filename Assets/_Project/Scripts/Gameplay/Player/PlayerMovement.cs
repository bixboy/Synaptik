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

    [Header("Fear Action Boost")]
    [SerializeField, Min(0f)]
    private float fearfulActionSpeedBonus = 3f;

    [SerializeField, Min(0f)]
    private float fearfulActionBoostDuration = 2f;

    [SerializeField, Min(0f)]
    private float fearfulActionBoostDecayDuration = 1f;

    [SerializeField, Min(0f)]
    private float fearfulActionBoostCooldown = 3f;

    [Header("Camera-relative ?")]
    [SerializeField]
    private bool cameraRelative = true;

    [SerializeField, ShowIf(nameof(cameraRelative))]
    private Camera targetCamera;

    [Header("Rotation")]
    [SerializeField, Min(0f)]
    private float rotationSpeed = 10f;

    private float currentSpeedBonus;
    private float speedBoostTimer;
    private float speedBoostCooldownTimer;
    private InputsDetection cachedInputsDetection;
    private bool isSubscribedToInputs;

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

    private void OnEnable()
    {
        TrySubscribeToInputs();
    }

    private void Start()
    {
        TrySubscribeToInputs();
    }

    private void Update()
    {
        if (!isSubscribedToInputs)
            TrySubscribeToInputs();
    }

    private void OnDisable()
    {
        TryUnsubscribeFromInputs();
    }

    private void FixedUpdate()
    {
        var input = InputsDetection.Instance ? InputsDetection.Instance.MoveVector : Vector2.zero;
        var direction = GetMovementDirection(input);

        UpdateSpeedBoost(Time.fixedDeltaTime);
        UpdateSpeedBoostCooldown(Time.fixedDeltaTime);

        var currentMaxSpeed = maxSpeed + currentSpeedBonus;

        var currentVelocity = rigidbodyComponent.linearVelocity;
        var horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        var targetHorizontalVelocity = direction * currentMaxSpeed;

        var currentAcceleration = direction.sqrMagnitude > 0.0001f ? acceleration : deceleration;
        var maxDeltaV = currentAcceleration * Time.fixedDeltaTime;
        var deltaV = targetHorizontalVelocity - horizontalVelocity;

        if (deltaV.sqrMagnitude > maxDeltaV * maxDeltaV)
            deltaV = deltaV.normalized * maxDeltaV;

        rigidbodyComponent.AddForce(deltaV, ForceMode.VelocityChange);

        UpdateRotation(direction, horizontalVelocity);
    }

    private void TrySubscribeToInputs()
    {
        if (isSubscribedToInputs)
            return;

        var instance = InputsDetection.Instance;
        if (!instance)
            return;

        instance.OnEmotionAction += HandleEmotionAction;
        cachedInputsDetection = instance;
        isSubscribedToInputs = true;
    }

    private void TryUnsubscribeFromInputs()
    {
        if (!isSubscribedToInputs)
            return;

        if (cachedInputsDetection)
            cachedInputsDetection.OnEmotionAction -= HandleEmotionAction;

        cachedInputsDetection = null;
        isSubscribedToInputs = false;
    }

    private void HandleEmotionAction(Emotion emotion, Behavior behavior)
    {
        if (emotion != Emotion.Fearful || behavior != Behavior.Action)
            return;

        if (speedBoostCooldownTimer > 0f)
            return;

        currentSpeedBonus = fearfulActionSpeedBonus;
        speedBoostTimer = fearfulActionBoostDuration;
        speedBoostCooldownTimer = fearfulActionBoostCooldown;
    }

    private void UpdateSpeedBoost(float deltaTime)
    {
        if (speedBoostTimer > 0f)
        {
            speedBoostTimer -= deltaTime;

            if (speedBoostTimer > 0f)
                return;

            speedBoostTimer = 0f;
        }

        if (currentSpeedBonus <= 0f)
            return;

        if (fearfulActionBoostDecayDuration <= 0f)
        {
            currentSpeedBonus = 0f;
            return;
        }

        float decayRate = fearfulActionSpeedBonus / fearfulActionBoostDecayDuration;
        currentSpeedBonus = Mathf.Max(0f, currentSpeedBonus - decayRate * deltaTime);
    }

    private void UpdateSpeedBoostCooldown(float deltaTime)
    {
        if (speedBoostCooldownTimer <= 0f)
            return;

        speedBoostCooldownTimer = Mathf.Max(0f, speedBoostCooldownTimer - deltaTime);
    }

    private Vector3 GetMovementDirection(Vector2 input)
    {
        if (!cameraRelative || !targetCamera)
            return new Vector3(input.x, 0f, input.y);
 
        Quaternion yawRotation = Quaternion.Euler(0f, targetCamera.transform.eulerAngles.y, 0f);
 
        Vector3 camForward = yawRotation * Vector3.forward;
        Vector3 camRight   = yawRotation * Vector3.right;
 
        Vector3 moveDir = camForward * input.y + camRight * input.x;
        return (moveDir.sqrMagnitude > 1f) ? moveDir.normalized : moveDir;
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
