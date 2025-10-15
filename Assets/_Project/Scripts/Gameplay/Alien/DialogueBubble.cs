using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialogueBubble : MonoBehaviour
{
    [SerializeField]
    private GameObject bubbleGameObject;
    
    [SerializeField]
    private Transform bubbleAnchor;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private Image bubbleImage;

    [SerializeField]
    private Sprite defaultBubbleSprite;

    [SerializeField]
    private EmotionBubbleSprite[] emotionSpecificSprites;

    [SerializeField]
    private bool lookAtCamera = true;

    [SerializeField, ShowIf(nameof(lookAtCamera))]
    private Camera targetCamera;

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

        if (!lookAtCamera)
            return;

        if (!targetCamera)
            targetCamera = Camera.main;

        if (!targetCamera)
            return;

        var forward = targetCamera.transform.rotation * Vector3.forward;
        var up = targetCamera.transform.rotation * Vector3.up;
        bubbleAnchor.transform.LookAt(bubbleAnchor.transform.position + forward, up);
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
