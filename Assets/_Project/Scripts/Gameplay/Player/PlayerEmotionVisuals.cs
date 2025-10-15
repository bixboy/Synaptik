using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class PlayerEmotionVisuals : MonoBehaviour
{
    [Header("Renderers")] 
    [SerializeField]
    private Renderer[] emotionRenderers = Array.Empty<Renderer>();

    [Header("Colors")] 
    [SerializeField]
    private Color defaultColor = Color.white;

    [SerializeField]
    private List<EmotionColorSetting> emotionColors = new()
    {
        new EmotionColorSetting
        {
            emotion = Emotion.Anger,
            color = new Color(240f / 255f, 83f / 255f, 83f / 255f)
        },
        new EmotionColorSetting
        {
            emotion = Emotion.Curious,
            color = new Color(127f / 255f, 213f / 255f, 93f / 255f)
        },
        new EmotionColorSetting
        {
            emotion = Emotion.Fearful,
            color = new Color(15f / 255f, 192f / 255f, 222f / 255f)
        },
        new EmotionColorSetting
        {
            emotion = Emotion.Friendly,
            color = new Color(255f / 255f, 221f / 255f, 97f / 255f)
        }
    };

    [SerializeField, Min(0f)]
    private float colorFadeDuration = 0.3f;

    private readonly List<Emotion> activeEmotions = new();
    private readonly Dictionary<Emotion, Color> emotionColorLookup = new();

    private InputsDetection cachedInputsDetection;
    private bool isSubscribed;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine colorFadeCoroutine;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [Serializable]
    private struct EmotionColorSetting
    {
        public Emotion emotion;
        public Color color;
    }

    private void Reset()
    {
        if (emotionRenderers == null || emotionRenderers.Length == 0)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer)
            {
                emotionRenderers = new[] { renderer };
            }
        }
    }

    private void Awake()
    {
        CacheEmotionColors();
        EnsureRenderers();
        ApplyColorImmediate(defaultColor);
    }

    private void OnEnable()
    {
        TrySubscribe();
        ApplyColorImmediate(GetColorForCurrentEmotion());
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void OnDestroy()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        var instance = InputsDetection.Instance;
        if (!instance)
            return;

        instance.OnEmotion += HandleEmotionInput;
        cachedInputsDetection = instance;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed)
            return;

        if (cachedInputsDetection)
        {
            cachedInputsDetection.OnEmotion -= HandleEmotionInput;
        }

        cachedInputsDetection = null;
        isSubscribed = false;
    }

    private void HandleEmotionInput(Emotion emotion, bool keyReleased)
    {
        if (emotion == Emotion.None)
            return;

        if (!keyReleased)
        {
            activeEmotions.Remove(emotion);
            activeEmotions.Add(emotion);
        }
        else
        {
            activeEmotions.RemoveAll(e => e == emotion);
        }

        var targetColor = GetColorForCurrentEmotion();
        FadeToColor(targetColor);
    }

    private Color GetColorForCurrentEmotion()
    {
        if (activeEmotions.Count == 0)
            return defaultColor;

        var latestEmotion = activeEmotions[activeEmotions.Count - 1];
        if (emotionColorLookup.TryGetValue(latestEmotion, out var color))
        {
            return color;
        }

        return defaultColor;
    }

    private void CacheEmotionColors()
    {
        emotionColorLookup.Clear();

        if (emotionColors == null)
            return;

        for (var i = 0; i < emotionColors.Count; i++)
        {
            var setting = emotionColors[i];
            if (setting.emotion == Emotion.None)
                continue;

            emotionColorLookup[setting.emotion] = setting.color;
        }
    }

    private void EnsureRenderers()
    {
        if (emotionRenderers != null && emotionRenderers.Length > 0)
            return;

        var renderer = GetComponent<Renderer>();
        if (renderer)
        {
            emotionRenderers = new[] { renderer };
        }
    }

    private void FadeToColor(Color targetColor)
    {
        if (emotionRenderers == null || emotionRenderers.Length == 0)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        if (colorFadeCoroutine != null)
            StopCoroutine(colorFadeCoroutine);

        if (colorFadeDuration <= 0f)
        {
            ApplyColorImmediate(targetColor);
            return;
        }

        colorFadeCoroutine = StartCoroutine(FadeRoutine(targetColor));
    }

    private IEnumerator FadeRoutine(Color targetColor)
    {
        var startColor = GetCurrentRendererColor();
        float elapsed = 0f;

        while (elapsed < colorFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / colorFadeDuration);
            var lerped = Color.Lerp(startColor, targetColor, Mathf.SmoothStep(0f, 1f, t));
            ApplyColorImmediate(lerped);
            yield return null;
        }

        ApplyColorImmediate(targetColor);
        colorFadeCoroutine = null;
    }

    private Color GetCurrentRendererColor()
    {
        if (emotionRenderers == null || emotionRenderers.Length == 0)
            return defaultColor;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        var renderer = emotionRenderers[0];
        if (!renderer)
            return defaultColor;

        renderer.GetPropertyBlock(propertyBlock);
        return propertyBlock.GetColor(BaseColorId);
    }

    private void ApplyColorImmediate(Color color)
    {
        if (emotionRenderers == null || emotionRenderers.Length == 0)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        for (var i = 0; i < emotionRenderers.Length; i++)
        {
            var renderer = emotionRenderers[i];
            if (!renderer)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
