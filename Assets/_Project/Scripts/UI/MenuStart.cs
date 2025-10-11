using Synaptik.Core;
using Synaptik.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Synaptik.UI
{
    public sealed class MenuStart : MonoBehaviour
    {
        [Header("Assign the UI")]
        [SerializeField]
        private RectTransform imageToShake;

        [SerializeField]
        private GameObject helpPanel;

        [SerializeField]
        private GameObject quitPanel;

        [Header("Shake Settings")]
        [SerializeField]
        private float maxShakeIntensity = 30f;

        [SerializeField]
        private float chargeTime = 3f;

        [SerializeField]
        private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Events")]
        public UnityEvent onFullyCharged;

        private float chargeProgress;
        private bool isCharging;
        private bool fullyCharged;
        private Vector3 originalPos;
        private bool panelHelpEnabled;
        private bool panelQuitEnabled;
        private InputsDetection inputsDetection;
        private bool subscribedToInputs;

        private void Awake()
        {
            if (imageToShake != null)
            {
                originalPos = imageToShake.anchoredPosition;
            }

            InitializePanels();
        }

        private void OnEnable()
        {
            TrySubscribeToInputs();
        }

        private void OnDisable()
        {
            UnsubscribeFromInputs();
        }

        private void InitializePanels()
        {
            if (helpPanel != null)
            {
                helpPanel.SetActive(false);
            }

            if (quitPanel != null)
            {
                quitPanel.SetActive(false);
            }

            panelHelpEnabled = false;
            panelQuitEnabled = false;
        }

        private bool TrySubscribeToInputs()
        {
            var instance = InputsDetection.Instance;
            if (instance == null)
            {
                return false;
            }

            if (inputsDetection == instance && subscribedToInputs)
            {
                return true;
            }

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
            if (!subscribedToInputs || inputsDetection == null)
            {
                return;
            }

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
                case Emotion.Curious when !keyUp:
                    ToggleHelpPanel();
                    break;
            }
        }

        private void HandleAction(Behavior action, bool isKeyUp)
        {
            if (isKeyUp)
            {
                return;
            }

            if (!panelQuitEnabled)
            {
                return;
            }

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
            if (!subscribedToInputs && !TrySubscribeToInputs())
            {
                return;
            }

            var comboActive = inputsDetection.MoveVector == Vector2.zero;
            if (!comboActive && isCharging)
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

            if (imageToShake != null)
            {
                var intensity = intensityCurve.Evaluate(chargeProgress) * maxShakeIntensity;
                var offset = Random.insideUnitCircle * intensity;
                imageToShake.anchoredPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
            }

            if (chargeProgress <= 0.001f && imageToShake != null)
            {
                imageToShake.anchoredPosition = originalPos;
            }
        }

        private void ToggleQuitPanel(bool enable)
        {
            panelQuitEnabled = enable;
            if (quitPanel != null)
            {
                quitPanel.SetActive(enable);
            }
        }

        private void ToggleHelpPanel()
        {
            if (helpPanel == null)
            {
                return;
            }

            panelHelpEnabled = !panelHelpEnabled;
            helpPanel.SetActive(panelHelpEnabled);
        }

        private void HandleQuitChoice(bool accept)
        {
            if (!panelQuitEnabled)
            {
                return;
            }

            if (accept)
            {
                Application.Quit();
                return;
            }

            ToggleQuitPanel(false);
        }

        public void TestStart()
        {
            Debug.Log("TestStart");
            LoadingScreenManager.Instance?.LoadScene("Test_Shahine");
        }
    }
}

