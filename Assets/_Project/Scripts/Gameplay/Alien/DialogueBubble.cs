using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public sealed class DialogueBubble : MonoBehaviour
{
    [SerializeField]
    private GameObject bubbleGameObject;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private EmotionBubbleSettings[] emotionSpecificBubbles;

    [SerializeField]
    private bool lookAtCamera = true;

    [SerializeField, ShowIf(nameof(lookAtCamera))]
    private Camera targetCamera;

    private float remainingTime;
    private readonly Dictionary<Emotion, BubbleInfo> bubbleLookup = new();
    private readonly List<GameObject> allBubbles = new();
    private BubbleInfo defaultBubble;
    private BubbleInfo activeBubble;

    [Serializable]
    private struct EmotionBubbleSettings
    {
        public Emotion emotion;
        public GameObject bubbleGameObject;
        public TextMeshProUGUI label;
    }

    private struct BubbleInfo
    {
        public GameObject BubbleObject;
        public TextMeshProUGUI Label;

        public bool IsValid => BubbleObject != null;
    }

    private void Awake()
    {
        targetCamera = Camera.main;
        CacheBubbles();
        Hide();
    }

    private void Update()
    {
        if (!activeBubble.BubbleObject || !activeBubble.BubbleObject.activeSelf)
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
        transform.LookAt(transform.position + forward, up);
    }

    public void ShowFor(Emotion emotion, string emojiLine, float duration)
    {
        if (string.IsNullOrEmpty(emojiLine) || duration <= 0f)
            return;

        var bubble = GetBubbleFor(emotion);
        if (!bubble.IsValid)
            return;

        if (activeBubble.BubbleObject && activeBubble.BubbleObject != bubble.BubbleObject)
            activeBubble.BubbleObject.SetActive(false);

        activeBubble = bubble;

        if (!activeBubble.Label && activeBubble.BubbleObject)
        {
            activeBubble.Label = activeBubble.BubbleObject.GetComponentInChildren<TextMeshProUGUI>(true);
            UpdateLookupLabel(emotion, activeBubble);
        }

        if (!activeBubble.Label && defaultBubble.Label)
            activeBubble.Label = defaultBubble.Label;

        if (activeBubble.Label)
            activeBubble.Label.text = emojiLine;

        activeBubble.BubbleObject.SetActive(true);

        remainingTime = duration;
    }

    private void Hide()
    {
        foreach (var bubble in allBubbles)
        {
            if (bubble)
                bubble.SetActive(false);
        }
        activeBubble = default;
        remainingTime = 0f;
    }

    private void CacheBubbles()
    {
        bubbleLookup.Clear();
        allBubbles.Clear();

        defaultBubble = CreateBubbleInfo(bubbleGameObject, label);
        if (defaultBubble.BubbleObject)
        {
            allBubbles.Add(defaultBubble.BubbleObject);
            if (!defaultBubble.Label)
            {
                defaultBubble.Label = defaultBubble.BubbleObject.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        if (emotionSpecificBubbles == null)
            return;

        foreach (var setting in emotionSpecificBubbles)
        {
            if (!setting.bubbleGameObject)
                continue;

            var info = CreateBubbleInfo(setting.bubbleGameObject, setting.label);

            if (!info.Label)
                info.Label = info.BubbleObject.GetComponentInChildren<TextMeshProUGUI>(true);

            bubbleLookup[setting.emotion] = info;
            allBubbles.Add(info.BubbleObject);
        }
    }

    private BubbleInfo GetBubbleFor(Emotion emotion)
    {
        if (bubbleLookup.TryGetValue(emotion, out var info))
        {
            return info;
        }

        if (defaultBubble.IsValid)
            return defaultBubble;

        return new BubbleInfo();
    }

    private void UpdateLookupLabel(Emotion emotion, BubbleInfo info)
    {
        if (bubbleLookup.ContainsKey(emotion))
            bubbleLookup[emotion] = info;
        else
            defaultBubble = info;
    }

    private static BubbleInfo CreateBubbleInfo(GameObject bubble, TextMeshProUGUI bubbleLabel)
    {
        return new BubbleInfo
        {
            BubbleObject = bubble,
            Label = bubbleLabel
        };
    }
}
