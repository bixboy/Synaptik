using UnityEngine;

public class MenuStart : MonoBehaviour
{
    [Header("Assign the UI")]
    public RectTransform imageToShake;
    public GameObject helpPanel;
    public GameObject quitPanel;

    [Header("Shake Settings")]
    public float maxShakeIntensity = 30f;
    public float chargeTime = 3f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onFullyCharged;

    private float chargeProgress;
    private bool isCharging;
    private bool fullyCharged;
    private Vector3 originalPos;
    
    private bool panelHelpEnable;
    private bool panelQuitEnable;

    private void Start()
    {
        if (imageToShake != null)
            originalPos = imageToShake.anchoredPosition;

        InputsDetection.Instance.OnEmotion += HandleEmotion;
        InputsDetection.Instance.OnAction += HandleAction;
        InputsDetection.Instance.OnTowActionPressed += HandleTowAction;

        if (helpPanel)
        {
            helpPanel.SetActive(false);
            panelHelpEnable = false;
        }

        if (quitPanel)
        {
            quitPanel.SetActive(false);
            panelQuitEnable = false;
        }
    }
    
    private void OnDestroy()
    {
        if (InputsDetection.Instance != null)
        {
            InputsDetection.Instance.OnEmotion -= HandleEmotion;
            InputsDetection.Instance.OnAction -= HandleAction;
            InputsDetection.Instance.OnTowActionPressed -= HandleTowAction;   
        }
    }

    private void HandleEmotion(Emotion emotion, bool keyUp)
    {
        if (emotion == Emotion.Anger)
        {
            Debug.Log("Quit");

            if (!keyUp)
            {
                quitPanel.SetActive(true);
                panelQuitEnable = true;   
            }
            else
            {
                quitPanel.SetActive(false);
                panelQuitEnable = false;
            }
        }

        if (emotion == Emotion.Curious)
        {
            Debug.Log("Help");
            ShowHelp();
        }
    }

    private void HandleAction(Behavior action, bool keyUp)
    {
        if (panelQuitEnable)
        {
            if (action == Behavior.Action)
            {
                Quit(true);
            }

            if (action == Behavior.Talking)
            {
                Quit(false);
            }
        }
    }

    private void HandleTowAction(bool towPressed)
    {
        if (towPressed)
        {
            StartCharging();
        }
        else
        {
            StopCharging();
        }
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

    private void Quit(bool accept)
    {
        if (panelQuitEnable)
        {
            if (accept)
            {
                Application.Quit();
            }
            else
            {
                quitPanel.SetActive(false);
                panelQuitEnable = false;
            }
        }
    }

    private void ShowHelp()
    {
        if (!helpPanel)
            return;
        
        if (!panelHelpEnable)
        {
            helpPanel.SetActive(true);
            panelHelpEnable = true;
        }
        else
        {
            helpPanel.SetActive(false);
            panelHelpEnable = false;
        }
    }
    
    public void TestStart()
    {
        Debug.Log("TestStart");
    }
}
