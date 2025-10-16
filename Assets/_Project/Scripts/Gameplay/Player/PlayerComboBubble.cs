using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[DisallowMultipleComponent]
public sealed class PlayerComboBubble : MonoBehaviour
{
    [Header("Bubble Setup")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float verticalOffset = 2.6f;
    [SerializeField] private float worldScale = 0.03f;

    [Header("Position Offset")]
    [Tooltip("Décale la bulle par rapport à sa position de base (local space)")]
    [SerializeField] private Vector3 bubbleOffset = Vector3.zero;

    [Header("Layout Settings")]
    [SerializeField] private Vector2 minBubbleSize = new(120f, 60f);
    [SerializeField] private Vector2 padding = new(16f, 10f);
    [SerializeField] private float defaultLifetime = 1.75f;

    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color textColor = Color.black;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Bubble Sprites")]
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Sprite defaultBubbleSprite;
    [SerializeField] private EmotionBubbleSprite[] emotionSpecificSprites;

    [Header("Sound")]
    [SerializeField] private VoicesModels _attributedVoice;
    [SerializeField] private StudioEventEmitter _soundEmitter;

    [Header("Distance Scaling")]
    [SerializeField, Min(0f)] private float minScale = 0.5f;
    [SerializeField, Min(0f)] private float maxScale = 1.5f;
    [SerializeField, Min(0f)] private float minDistance = 2f;
    [SerializeField, Min(0f)] private float maxDistance = 15f;

    private GameObject bubbleInstance;
    private RectTransform bubbleRect;
    private Image backgroundImage;
    private TextMeshProUGUI label;
    private float remainingTime;
    private Camera targetCamera;

    private readonly Dictionary<Emotion, Sprite> spriteLookup = new();
    private Sprite activeSprite;

    [Serializable]
    private struct EmotionBubbleSprite
    {
        public Emotion emotion;
        public Sprite sprite;
    }

    private void Awake()
    {
        targetCamera = Camera.main;
        HideImmediate();
    }

    private void OnValidate()
    {
        CacheSprites();

        if (!bubblePrefab || bubbleImage)
            return;

        bubbleImage = bubblePrefab.GetComponentInChildren<Image>(true);
    }

    private void OnDisable()
    {
        HideImmediate();
    }

    private void LateUpdate()
    {
        if (remainingTime <= 0f)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            HideImmediate();
            return;
        }

        UpdateLookAt();
        UpdateScale();
        UpdateOffset(); // 🟢 Nouvelle ligne
    }

    public void Show(Emotion emotion, string text, float duration)
    {
        EnsureInstance();

        if (!label)
            return;

        label.text = text ?? string.Empty;
        AdjustBubbleSize();
        ApplyBubbleSprite(emotion);

        if (bubbleInstance && !bubbleInstance.activeSelf)
            bubbleInstance.SetActive(true);
        
        
        EventReference  eventReference = SoundManager.Instance.GetVoice(emotion, _attributedVoice);
        if (_soundEmitter && !eventReference.IsNull)
        {
            _soundEmitter.EventReference = eventReference;
            _soundEmitter.Play();
        }
        else
            Debug.LogError($"Sound Emitter missing : {gameObject.name}", gameObject);

        remainingTime = duration > 0f ? duration : defaultLifetime;
        UpdateLookAt();
        UpdateScale();
        UpdateOffset(); // 🟢 Application directe à l’apparition
    }

    public void HideImmediate()
    {
        remainingTime = 0f;
        activeSprite = null;
        if (bubbleInstance)
            bubbleInstance.SetActive(false);
    }

    private void EnsureInstance()
    {
        if (bubbleInstance)
            return;

        if (!bubblePrefab)
        {
            Debug.LogError("[PlayerComboBubble] No bubble prefab assigned!");
            return;
        }

        targetCamera = Camera.main;

        bubbleInstance = Instantiate(bubblePrefab, transform);
        bubbleRect = bubbleInstance.GetComponent<RectTransform>();
        bubbleRect.localPosition = new Vector3(0f, verticalOffset, 0f) + bubbleOffset; // 🟢 offset ajouté ici
        bubbleRect.localScale = Vector3.one * Mathf.Max(0.0001f, worldScale);
        bubbleRect.pivot = new Vector2(0.5f, 0f);

        label = bubbleInstance.GetComponentInChildren<TextMeshProUGUI>(true);
        backgroundImage = bubbleInstance.GetComponentInChildren<Image>(true);

        if (!bubbleImage)
            bubbleImage = backgroundImage;

        if (label)
        {
            label.color = textColor;
            label.font = fontAsset ? fontAsset : TMP_Settings.defaultFontAsset;
        }

        if (backgroundImage)
            backgroundImage.color = backgroundColor;

        if (!defaultBubbleSprite && bubbleImage)
            defaultBubbleSprite = bubbleImage.sprite;

        bubbleInstance.SetActive(false);
    }

    private void AdjustBubbleSize()
    {
        if (!label || !bubbleRect)
            return;

        label.ForceMeshUpdate();
        var textSize = label.GetPreferredValues(label.text);

        var finalSize = new Vector2(
            Mathf.Max(minBubbleSize.x, textSize.x + padding.x * 2f),
            Mathf.Max(minBubbleSize.y, textSize.y + padding.y * 2f)
        );

        bubbleRect.sizeDelta = finalSize;
    }

    private void UpdateLookAt()
    {
        if (!bubbleRect)
            return;

        if (!targetCamera)
            targetCamera = Camera.main;

        if (!targetCamera)
            return;

        var forward = targetCamera.transform.rotation * Vector3.forward;
        var up = targetCamera.transform.rotation * Vector3.up;
        bubbleRect.rotation = Quaternion.LookRotation(forward, up);
    }

    private void UpdateScale()
    {
        if (!bubbleRect || !targetCamera)
            return;

        float distance = Vector3.Distance(targetCamera.transform.position, bubbleRect.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float scale = Mathf.Lerp(maxScale, minScale, t);
        bubbleRect.localScale = Vector3.one * scale * worldScale;
    }

    // 🟢 Fonction ajoutée : applique le décalage défini dans l’inspecteur
    private void UpdateOffset()
    {
        if (!bubbleRect)
            return;

        // On garde le verticalOffset et on ajoute bubbleOffset
        bubbleRect.localPosition = new Vector3(0f, verticalOffset, 0f) + bubbleOffset;
    }

    private void CacheSprites()
    {
        spriteLookup.Clear();

        if (emotionSpecificSprites == null)
            return;

        foreach (var setting in emotionSpecificSprites)
        {
            if (!setting.sprite)
                continue;

            spriteLookup[setting.emotion] = setting.sprite;
        }
    }

    private void ApplyBubbleSprite(Emotion emotion)
    {
        if (!bubbleImage)
            return;

        var sprite = GetSpriteFor(emotion);
        if (!sprite || sprite == activeSprite)
            return;

        bubbleImage.sprite = sprite;
        activeSprite = sprite;
    }

    private Sprite GetSpriteFor(Emotion emotion)
    {
        if (spriteLookup.TryGetValue(emotion, out var sprite))
            return sprite;

        if (defaultBubbleSprite)
            return defaultBubbleSprite;

        return bubbleImage ? bubbleImage.sprite : null;
    }
}
