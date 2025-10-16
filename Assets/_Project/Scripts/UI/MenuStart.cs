using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public sealed class MenuStart : MonoBehaviour
{
    [Header("Assign the UI")]
    [SerializeField] private RectTransform imageToShake;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject quitPanel;

    [Header("Shake Settings")]
    [SerializeField] private float maxShakeIntensity = 30f;
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent onFullyCharged;

    [Header("Barometer Settings")]
    [Tooltip("Aiguille du baromètre à faire pivoter.")]
    [SerializeField] private RectTransform needleTransform;

    [Tooltip("Rotation de l’aiguille quand charge = 0")]
    [SerializeField] private float needleMinRotation = 90f;

    [Tooltip("Rotation de l’aiguille quand charge = 100%")]
    [SerializeField] private float needleMaxRotation = -90f;

    [Tooltip("Vitesse de retour de l’aiguille à 0 quand la charge descend.")]
    [SerializeField] private float needleReturnSpeed = 3f;

    [Header("SFX")]
    [SerializeField] private StudioEventEmitter _startEmitter;
    
    [SerializeField] private EventReference _music;
    [SerializeField] private EventReference _ambiant;

    private float chargeProgress;
    private bool isCharging;
    private bool fullyCharged;
    private Vector3 originalPos;

    private bool panelHelpEnabled;
    private bool panelQuitEnabled;
    private InputsDetection inputsDetection;
    private bool subscribedToInputs;

    private bool windowOpened;

    private void Awake()
    {
        if (imageToShake != null)
            originalPos = imageToShake.anchoredPosition;

        InitializePanels();
    }

    private void OnEnable() => TrySubscribeToInputs();
    private void OnDisable() => UnsubscribeFromInputs();

    private void InitializePanels()
    {
        if (helpPanel) helpPanel.SetActive(false);
        if (quitPanel) quitPanel.SetActive(false);
        
        panelHelpEnabled = false;
        panelQuitEnabled = false;
    }

    private bool TrySubscribeToInputs()
    {
        var instance = InputsDetection.Instance;
        if (!instance) 
            return false;

        if (inputsDetection == instance && subscribedToInputs)
            return true;

        UnsubscribeFromInputs();

        inputsDetection = instance;
        inputsDetection.OnEmotion += HandleEmotion;
        inputsDetection.OnAction += HandleAction;
        inputsDetection.OnTowActionPressed += HandleTwoAction;
        subscribedToInputs = true;

        return true;
    }

    private void UnsubscribeFromInputs()
    {
        if (!subscribedToInputs || !inputsDetection)
            return;

        inputsDetection.OnEmotion -= HandleEmotion;
        inputsDetection.OnAction -= HandleAction;
        inputsDetection.OnTowActionPressed -= HandleTwoAction;
        subscribedToInputs = false;
    }

    private void HandleEmotion(Emotion emotion, bool keyUp)
    {
        switch (emotion)
        {
            case Emotion.Anger:
                ToggleQuitPanel(!keyUp);
                break;
            
            case Emotion.Curious:
                ToggleHelpPanel();
                break;
        }
    }

    private void HandleAction(Behavior action, bool isKeyUp)
    {
        if (isKeyUp || !panelQuitEnabled) 
            return;

        switch (action)
        {
            case Behavior.Action:
                HandleQuitChoice(true);
                break;
            
            case Behavior.Talking:
                HandleQuitChoice(false);
                break;
        }
    }

    private void HandleTwoAction(bool towPressed)
    {
        if (towPressed) 
            StartCharging();
        else 
            StopCharging();
    }

    private void StartCharging()
    {
        isCharging = true;
        if (_startEmitter)
            _startEmitter.Play();
        
        fullyCharged = false;
    }

    private void StopCharging()
    {
        isCharging = false;
        if (_startEmitter)
            _startEmitter.Stop();
    }

    private void Update()
    {
        if (!subscribedToInputs && !TrySubscribeToInputs())
            return;

        var comboActive = inputsDetection.MoveVector == Vector2.zero;
        if (!comboActive && isCharging)
            StopCharging();

        // ---- CHARGE ----
        if (isCharging && !fullyCharged)
        {
            chargeProgress += Time.deltaTime / chargeTime;
            
            if (chargeProgress >= 1f)
            {
                chargeProgress = 1f;
                fullyCharged = true;
                
                onFullyCharged?.Invoke();
                if (_startEmitter)
                    _startEmitter.Stop();
            }
        }
        else
        {
            chargeProgress -= Time.deltaTime;
        }

        chargeProgress = Mathf.Clamp01(chargeProgress);

        // ---- SOUND ----
        // if (_startEmitter != null)
        //     _startEmitter.SetParameter("fuck", chargeProgress);
        
        // ---- SHAKE ----
        if (imageToShake)
        {
            var intensity = intensityCurve.Evaluate(chargeProgress) * maxShakeIntensity;
            var offset = Random.insideUnitCircle * intensity;
            imageToShake.anchoredPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
        }

        if (chargeProgress <= 0.001f && imageToShake)
            imageToShake.anchoredPosition = originalPos;

        // ---- BAROMETER NEEDLE ----
        if (needleTransform)
        {
            float targetRotation = Mathf.Lerp(needleMinRotation, needleMaxRotation, chargeProgress);

            float currentRotation = Mathf.LerpAngle(
                needleTransform.localEulerAngles.z,
                targetRotation,
                Time.deltaTime * needleReturnSpeed
            );

            // --- VIBRATION / JITTER réaliste ---
            float vibrationStart = 0.1f;
            float vibrationProgress = Mathf.InverseLerp(vibrationStart, 1f, chargeProgress * 1.5f);
            float vibrationIntensity = Mathf.SmoothStep(0f, 1f, vibrationProgress);

            float slowNoise = (Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f) * 2f;
            float fastSine = Mathf.Sin(Time.time * 300f) * 0.25f;
            float modulated = fastSine * (0.3f + Mathf.Abs(slowNoise) * 0.7f);

            float jitterAmplitude = 1.5f;
            float jitter = modulated * jitterAmplitude * Mathf.Pow(vibrationIntensity, 1.5f);
            needleTransform.localEulerAngles = new Vector3(0f, 0f, currentRotation + jitter);
        }
    }

    private void ToggleQuitPanel(bool enable)
    {
        if (!quitPanel || panelHelpEnabled)
            return;
        
        panelQuitEnabled = enable;
        
        if (enable)
            SoundManager.Instance.UIValid();
        else
            SoundManager.Instance.UIInvalid();
        
        quitPanel.SetActive(enable);
        
    }

    private void ToggleHelpPanel()
    {
        if (!helpPanel || panelQuitEnabled) 
            return;
        
        panelHelpEnabled = !panelHelpEnabled;
        
        if (panelHelpEnabled)
            SoundManager.Instance.UIValid();
        else
            SoundManager.Instance.UIInvalid();
        
        helpPanel.SetActive(panelHelpEnabled);
        
    }

    private void HandleQuitChoice(bool accept)
    {
        if (!panelQuitEnabled) 
            return;

        if (accept)
        {
            Application.Quit();
        }
        else
        {
            ToggleQuitPanel(false);
        }
    }

    public void TestStart()
    {
        Debug.Log("TestStart");
        LoadingScreenManager.Instance?.LoadScene("Test_Shahine");
    }
}
