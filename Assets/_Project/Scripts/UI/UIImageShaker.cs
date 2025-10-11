using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIObjectShaker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField, Tooltip("Le GameObject UI à faire trembler (doit avoir un RectTransform)")]
    private GameObject targetUI;

    [Header("Shake Settings")]
    [SerializeField]
    private float shakePower = 10f;

    [SerializeField]
    private float shakeSpeed = 25f;

    [SerializeField]
    private float shakeDuration = 1f;

    [Header("Floating Number Settings")]

    [SerializeField]
    private GameObject floatingNumberPrefab;

    [SerializeField]
    private RectTransform floatingNumberSpawn;

    [SerializeField]
    private Vector2 floatOffset = new(0f, 60f);

    [SerializeField]
    private float floatDuration = 1f;

    [SerializeField]
    private float floatRandomAngle = 25f;

    [SerializeField]
    private float floatRandomSpeedFactor = 0.25f;

    private RectTransform targetRect;
    private Vector3 originalPosition;
    private float shakeTimer;
    private bool isShaking;

    private void Awake()
    {
        if (targetUI != null)
        {
            targetRect = targetUI.GetComponent<RectTransform>();
        }

        if (targetRect != null)
        {
            originalPosition = targetRect.anchoredPosition;
        }
    }

    private void Start()
    {
        if (MistrustManager.Instance != null)
        {
            MistrustManager.Instance.OnMistrust += HandleMistrust;
        }
    }

    private void OnDestroy()
    {
        if (MistrustManager.Instance != null)
        {
            MistrustManager.Instance.OnMistrust -= HandleMistrust;
        }
    }

    private void Update()
    {
        if (!isShaking || !targetRect)
            return;

        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0f)
        {
            StopShake();
            return;
        }

        var offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * 2f * shakePower;
        var offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * 2f * shakePower;

        targetRect.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    private void HandleMistrust(float value)
    {
        Shake(shakeDuration);
        SpawnFloatingNumber(value);
    }

    public void Shake(float duration)
    {
        if (!targetRect)
        {
            Debug.LogWarning("[UIObjectShaker] Aucun RectTransform assigné !");
            return;
        }

        shakeTimer = duration;
        isShaking = true;
    }

    public void StopShake()
    {
        isShaking = false;
        shakeTimer = 0f;

        if (targetRect)
            targetRect.anchoredPosition = originalPosition;
    }

    public void RecalibrateOrigin()
    {
        if (targetRect)
            originalPosition = targetRect.anchoredPosition;
    }

    private void SpawnFloatingNumber(float value)
    {
        if (!floatingNumberPrefab || !floatingNumberSpawn)
        {
            Debug.LogWarning("[UIObjectShaker] Pas de prefab ou de point de spawn assigné !");
            return;
        }

        var instance = Instantiate(floatingNumberPrefab, floatingNumberSpawn);
        var rect = instance.GetComponentInChildren<RectTransform>();
        var text = instance.GetComponentInChildren<TextMeshProUGUI>();

        if (!rect || !text)
        {
            Debug.LogWarning("[UIObjectShaker] Le prefab du nombre flottant doit contenir un TextMeshProUGUI !");
            Destroy(instance);
            return;
        }

        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        instance.SetActive(true);

        if (value > 0f)
        {
            text.text = "+";
            text.color = Color.green;
        }
        else if (value < 0f)
        {
            text.text = "−";
            text.color = Color.red;
        }
        else
        {
            text.text = "";
            text.color = Color.white;
        }

        text.alpha = 1f;

        var randomAngle = Random.Range(-floatRandomAngle, floatRandomAngle);
        var randomDir = Quaternion.Euler(0f, 0f, randomAngle) * floatOffset;
        var randomSpeedFactor = Random.Range(1f - floatRandomSpeedFactor, 1f + floatRandomSpeedFactor);
        var finalDuration = Mathf.Max(0.1f, floatDuration * randomSpeedFactor);

        StartCoroutine(AnimateFloatingNumber(rect, text, randomDir, finalDuration));
    }

    private IEnumerator AnimateFloatingNumber(RectTransform rect, TextMeshProUGUI text, Vector2 offset, float duration)
    {
        var elapsed = 0f;
        var startPos = rect.anchoredPosition;
        var targetPos = startPos + offset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            text.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(rect.gameObject);
    }
}
