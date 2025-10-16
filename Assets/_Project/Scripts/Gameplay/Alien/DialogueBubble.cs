using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialogueBubble : MonoBehaviour
{
    [SerializeField] private GameObject bubbleGameObject;
    [SerializeField] private Transform bubbleAnchor;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Sprite defaultBubbleSprite;
    [SerializeField] private EmotionBubbleSprite[] emotionSpecificSprites;
    [SerializeField] private bool lookAtCamera = true;
    [SerializeField, ShowIf(nameof(lookAtCamera))] private Camera targetCamera;

    // 🟢 --- Nouveau : paramètres du scale selon la distance ---
    [Header("Distance Scaling")]
    [SerializeField, Min(0f)] private float minScale = 0.5f;
    [SerializeField, Min(0f)] private float maxScale = 1.5f;
    [SerializeField, Min(0f)] private float minDistance = 2f;
    [SerializeField, Min(0f)] private float maxDistance = 15f;
    // -----------------------------------------------------------

    private float remainingTime;
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
        CacheBubbleComponents();
        CacheSprites();
        Hide();
    }

    private void OnValidate()
    {
        CacheBubbleComponents();
        CacheSprites();
    }

    private void Update()
    {
        if (!bubbleGameObject || !bubbleGameObject.activeSelf)
            return;

        if (remainingTime <= 0f)
        {
            Hide();
            return;
        }

        remainingTime -= Time.deltaTime;

        if (!targetCamera)
            targetCamera = Camera.main;

        if (!targetCamera)
            return;

        // 🌀 Look at camera
        if (lookAtCamera && bubbleAnchor)
        {
            var forward = targetCamera.transform.rotation * Vector3.forward;
            var up = targetCamera.transform.rotation * Vector3.up;
            bubbleAnchor.transform.LookAt(bubbleAnchor.position + forward, up);
        }

        // 📏 Scale selon la distance
        float distance = Vector3.Distance(targetCamera.transform.position, bubbleAnchor.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float scale = Mathf.Lerp(maxScale, minScale, t); // plus proche = plus grand
        bubbleAnchor.localScale = Vector3.one * scale;
    }

    public void ShowFor(Emotion emotion, string emojiLine, float duration)
    {
        if (string.IsNullOrEmpty(emojiLine) || duration <= 0f)
            return;

        if (!bubbleGameObject)
            return;

        var sprite = GetSpriteFor(emotion);

        if (bubbleImage && sprite && sprite != activeSprite)
        {
            bubbleImage.sprite = sprite;
            activeSprite = sprite;
        }

        if (label)
            label.text = emojiLine;

        bubbleGameObject.SetActive(true);
        remainingTime = duration;
    }

    private void Hide()
    {
        if (bubbleGameObject)
            bubbleGameObject.SetActive(false);

        activeSprite = null;
        remainingTime = 0f;
    }

    private void CacheBubbleComponents()
    {
        if (!bubbleGameObject)
            bubbleGameObject = gameObject;

        if (!label && bubbleGameObject)
            label = bubbleGameObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (!bubbleImage && bubbleGameObject)
            bubbleImage = bubbleGameObject.GetComponentInChildren<Image>(true);

        if (!defaultBubbleSprite && bubbleImage)
            defaultBubbleSprite = bubbleImage.sprite;
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

    private Sprite GetSpriteFor(Emotion emotion)
    {
        if (spriteLookup.TryGetValue(emotion, out var sprite))
            return sprite;

        if (defaultBubbleSprite)
            return defaultBubbleSprite;

        return bubbleImage ? bubbleImage.sprite : null;
    }
}
