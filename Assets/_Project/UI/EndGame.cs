using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public sealed class EndGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image circleFill;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI subText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeBeep;
    [SerializeField] private AudioClip successJingle;
    [SerializeField] private AudioClip failureAlarm;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float circleDuration = 2.5f;
    [SerializeField] private float charDelay = 0.03f;
    [SerializeField] private float lineDelay = 0.25f;
    [SerializeField] private float circlePulseScale = 1.1f;
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float shakeIntensity = 10f;
    [SerializeField] private Color winColor = Color.green;
    [SerializeField] private Color loseColor = Color.red;

    [Header("Lines")]
    [SerializeField] private List<string> winLines = new List<string>();
    [SerializeField] private List<string> loseLines = new List<string>();

    private Coroutine currentRoutine;
    private Vector3 initialCircleScale;

    private void Awake()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (circleFill)
        {
            circleFill.fillAmount = 0f;
            initialCircleScale = circleFill.transform.localScale;
        }
    }

    public void ShowWinSequence()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(WinRoutine());
    }

    public void ShowLoseSequence()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(LoseRoutine());
    }

    // ======================
    // WIN SEQUENCE
    // ======================
    private IEnumerator WinRoutine()
    {
        mainText.text = "";
        subText.text = "";
        yield return FadeCanvas(1f);
        if (successJingle) audioSource?.PlayOneShot(successJingle);

        // Circle animation with pulse
        circleFill.color = winColor;
        float t = 0f;
        while (t < circleDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / circleDuration);
            circleFill.fillAmount = progress;

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.05f;
            circleFill.transform.localScale = initialCircleScale * Mathf.Lerp(1f, circlePulseScale, pulse);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);

        foreach (string line in winLines)
        {
            yield return StartCoroutine(TypeLine($"> {line}\n", winColor));
            yield return new WaitForSecondsRealtime(lineDelay);
        }

        subText.text = "<color=#00FFAA>SIMULATION COMPLETE</color>";
        yield return new WaitForSecondsRealtime(2f);

        yield return FadeCanvas(0f);
        circleFill.transform.localScale = initialCircleScale;

        LoadMainMenu();
    }

    // ======================
    // LOSE SEQUENCE
    // ======================
    private IEnumerator LoseRoutine()
    {
        mainText.text = "";
        subText.text = "";
        yield return FadeCanvas(1f);
        if (failureAlarm) audioSource?.PlayOneShot(failureAlarm);

        Coroutine warningRoutine = StartCoroutine(SystemFailureWarning());
        Coroutine typingRoutine = StartCoroutine(TypeLoseText());

        yield return typingRoutine;
        StopCoroutine(warningRoutine);

        yield return new WaitForSecondsRealtime(1.5f);
        yield return FadeCanvas(0f);

            LoadMainMenu();
    }

    private IEnumerator TypeLoseText()
    {
        Vector3 startPos = mainText.transform.localPosition;

        foreach (string line in loseLines)
        {
            yield return StartCoroutine(TypeLine($"> {line}", loseColor, shake: true));
            mainText.text += "\n";
            yield return new WaitForSecondsRealtime(lineDelay);
        }

        mainText.transform.localPosition = startPos;
    }

    private IEnumerator SystemFailureWarning()
    {
        float blinkSpeed = 3f;
        Color c = loseColor;

        while (true)
        {
            float intensity = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            subText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.Lerp(Color.black, c, intensity))}>⚠ SYSTEM FAILURE ⚠</color>";
            yield return null;
        }
    }
    
    private IEnumerator TypeLine(string line, Color color, bool shake = false)
    {
        string colorTagStart = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
        string colorTagEnd = "</color>";
        Vector3 startPos = mainText.transform.localPosition;

        for (int i = 0; i < line.Length; i++)
        {
            // Ajoute le texte et le ferme correctement pour éviter de casser le style
            mainText.text += $"{colorTagStart}{line[i]}{colorTagEnd}";

            if (typeBeep)
                audioSource?.PlayOneShot(typeBeep, 0.2f);

            if (shake)
            {
                Vector3 j = Random.insideUnitSphere * (shakeIntensity * 0.08f);
                mainText.transform.localPosition = startPos + j;
            }

            yield return new WaitForSecondsRealtime(charDelay);
        }

        if (shake)
            mainText.transform.localPosition = startPos;
    }

    // ======================
    // CANVAS FADE
    // ======================
    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float t = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = targetAlpha > 0f ? Vector3.one : Vector3.one * 1.05f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        transform.localScale = Vector3.one;
    }
    
    
    private void LoadMainMenu()
    {
        if (LoadingScreenManager.Instance)
        {
            LoadingScreenManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("[EndGameUI] LoadingScreenManager.Instance est null, retour direct à la scène MainMenu.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
