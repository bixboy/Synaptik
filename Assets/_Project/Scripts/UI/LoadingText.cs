using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class LoadingText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Typing Settings")]
    [SerializeField] private float minCharDelay = 0.01f;
    [SerializeField] private float maxCharDelay = 0.05f;
    [SerializeField] private bool randomizeDelay = true;
    [SerializeField] private bool showCursor = true;
    [SerializeField] private string cursorSymbol = "_";

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private float typeSoundVolume = 0.1f;

    [Header("Loading Animation")]
    [SerializeField] private string loadingPattern = "LOADING";
    [SerializeField] private float loadingCycleSpeed = 0.3f;

    [Header("Cursor Blink")]
    [SerializeField] private float cursorBlinkSpeed = 0.5f;

    private AudioSource audioSource;
    private Coroutine typingCoroutine;
    private Coroutine loopCoroutine;
    private Coroutine cursorCoroutine;

    private bool stopTyping;
    private bool cursorVisible = true;
    private float currentProgress = 0f;
    private int dotCount = 0;

    private string currentText = "";

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (mainText) mainText.text = "";
        if (loadingText) loadingText.text = "";
        if (progressText) progressText.text = "";
    }

    public void StartText(IReadOnlyList<string> lines)
    {
        StopAllCoroutines();
        typingCoroutine = StartCoroutine(DisplayLinesRoutine(lines));
        if (showCursor)
            cursorCoroutine = StartCoroutine(CursorBlinkRoutine());
    }

    private IEnumerator DisplayLinesRoutine(IReadOnlyList<string> lines)
    {
        stopTyping = false;
        if (mainText) mainText.text = "";

        currentText = "";

        foreach (var line in lines)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (stopTyping) yield break;

                currentText += line[i];
                UpdateDisplayedText();

                if (typeSound)
                    audioSource.PlayOneShot(typeSound, typeSoundVolume);

                float delay = randomizeDelay ? Random.Range(minCharDelay, maxCharDelay) : minCharDelay;
                yield return new WaitForSeconds(delay);
            }

            currentText += "\n";
        }

        loopCoroutine = StartCoroutine(LoadingLoop());
    }

    private IEnumerator LoadingLoop()
    {
        while (!stopTyping)
        {
            dotCount = (dotCount + 1) % 4;
            string dots = new string('.', dotCount);

            if (loadingText)
                loadingText.text = $"{loadingPattern}{dots}";

            if (progressText)
            {
                float progress = Mathf.Clamp01(currentProgress);
                progress *= 1.2f;
                progressText.text = $"[{TextFeedBack.ProgressiveDisplayLerp("000000000000000000000000", progress, '-')}]";
            }

            yield return new WaitForSeconds(loadingCycleSpeed);
        }
    }

    private IEnumerator CursorBlinkRoutine()
    {
        while (!stopTyping)
        {
            cursorVisible = !cursorVisible;
            UpdateDisplayedText();
            yield return new WaitForSeconds(cursorBlinkSpeed);
        }
    }

    private void UpdateDisplayedText()
    {
        if (!mainText) return;

        if (showCursor && cursorVisible)
            mainText.text = currentText + cursorSymbol;
        else
            mainText.text = currentText;
    }

    public void SetLoadingProgress(float lerp)
    {
        currentProgress = Mathf.Clamp01(lerp);
    }

    public void StopAll()
    {
        stopTyping = true;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);
        
        if (cursorCoroutine != null)
            StopCoroutine(cursorCoroutine);

        if (loadingText) loadingText.text = "";
        if (progressText) progressText.text = "";
    }
}
