using UnityEngine;

public class MenuStart : MonoBehaviour
{
    [Header("Assign the UI Image to shake")]
    public RectTransform imageToShake;

    [Header("Shake Settings")]
    public float maxShakeIntensity = 30f;
    public float chargeTime = 3f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onFullyCharged;

    private float chargeProgress = 0f;
    private bool isCharging = false;
    private bool fullyCharged = false;
    private Vector3 originalPos;

    private void Start()
    {
        if (imageToShake != null)
            originalPos = imageToShake.anchoredPosition;

        InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
    }
    
    private void OnDestroy()
    {
        if (InputsDetection.Instance != null)
            InputsDetection.Instance.OnEmotionAction -= HandleEmotionAction;
    }

    private void HandleEmotionAction(Emotion emotion, Behavior action)
    {
        
        Debug.Log("Test");
        StartCharging();
    }
    
    private void StartCharging()
    {
        isCharging = true;
        fullyCharged = false;
    }

    private void StopCharging()
    {
        isCharging = false;
    }

    private void Update()
    {
        if (!InputsDetection.Instance) 
            return;
        
        bool comboActif = InputsDetection.Instance.MoveVector == Vector2.zero;
        if (!comboActif && isCharging)
        {
            StopCharging();
        }

        if (isCharging && !fullyCharged)
        {
            chargeProgress += Time.deltaTime / chargeTime;
            if (chargeProgress >= 1f)
            {
                chargeProgress = 1f;
                fullyCharged = true;
                onFullyCharged?.Invoke();
            }
        }
        else
        {
            chargeProgress -= Time.deltaTime;
        }

        chargeProgress = Mathf.Clamp01(chargeProgress);

        if (imageToShake)
        {
            float intensity = intensityCurve.Evaluate(chargeProgress) * maxShakeIntensity;
            Vector2 offset = Random.insideUnitCircle * intensity;
            imageToShake.anchoredPosition = originalPos + new Vector3(offset.x, offset.y, 0);
        }

        if (chargeProgress <= 0.001f && imageToShake)
        {
            imageToShake.anchoredPosition = originalPos;
        }
    }
}
