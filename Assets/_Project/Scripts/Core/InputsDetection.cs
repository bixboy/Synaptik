using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système de détection combinée : émotion + action
/// Quand une touche "émotion" et une touche "action" sont pressées simultanément,
/// un callback global est déclenché.
/// </summary>
public class InputsDetection : MonoBehaviour
{
   public static InputsDetection Instance { get; private set; }

    [Header("Configuration des touches ↔ émotions")]
    public List<KeyEmotionBinding> emotionBindings = new List<KeyEmotionBinding>
    {
        new KeyEmotionBinding(KeyCode.F1, Emotion.Anger),
        new KeyEmotionBinding(KeyCode.F2, Emotion.Curious),
        new KeyEmotionBinding(KeyCode.F3, Emotion.Fearful),
        new KeyEmotionBinding(KeyCode.F4, Emotion.Friendly),
    };

    [Header("Configuration des touches ↔ actions")]
    public List<KeyActionBinding> actionBindings = new List<KeyActionBinding>
    {
        new KeyActionBinding(KeyCode.F5, Behavior.Talking),
        new KeyActionBinding(KeyCode.F6, Behavior.Action)
    };

    public delegate void EmotionActionDelegate(Emotion emotion, Behavior action);
    public event EmotionActionDelegate OnEmotionAction;

    private Dictionary<KeyCode, Emotion> _emotionMap;
    private Dictionary<KeyCode, Behavior> _actionMap;

    private HashSet<KeyCode> _pressedEmotionKeys = new HashSet<KeyCode>();
    private HashSet<KeyCode> _pressedActionKeys = new HashSet<KeyCode>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _emotionMap = new Dictionary<KeyCode, Emotion>();
        _actionMap = new Dictionary<KeyCode, Behavior>();

        foreach (var e in emotionBindings)
            if (!_emotionMap.ContainsKey(e.key))
                _emotionMap.Add(e.key, e.emotion);

        foreach (var a in actionBindings)
            if (!_actionMap.ContainsKey(a.key))
                _actionMap.Add(a.key, a.action);

        Debug.Log($"[InputsDetection] {_emotionMap.Count} émotions / {_actionMap.Count} actions configurées.");
    }

    void Update()
    {
        // --- Détection des émotions ---
        foreach (var kvp in _emotionMap)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                _pressedEmotionKeys.Add(kvp.Key);
                TryTriggerCombos(kvp.Value);
            }
            if (Input.GetKeyUp(kvp.Key))
            {
                _pressedEmotionKeys.Remove(kvp.Key);
            }
        }

        // --- Détection des actions ---
        foreach (var kvp in _actionMap)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                _pressedActionKeys.Add(kvp.Key);
                TryTriggerCombos(action: kvp.Value);
            }
            if (Input.GetKeyUp(kvp.Key))
            {
                _pressedActionKeys.Remove(kvp.Key);
            }
        }
    }

    private void TryTriggerCombos(Emotion emotion = Emotion.None, Behavior action = Behavior.None)
    {
        if (emotion != Emotion.None)
        {
            foreach (var aKey in _pressedActionKeys)
            {
                Behavior activeAction = _actionMap[aKey];
                Trigger(emotion, activeAction);
            }
        }

        if (action != Behavior.None)
        {
            foreach (var eKey in _pressedEmotionKeys)
            {
                Emotion activeEmotion = _emotionMap[eKey];
                Trigger(activeEmotion, action);
            }
        }
    }

    private void Trigger(Emotion emotion, Behavior action)
    {
        Debug.Log($"🎭 Combo détecté → Émotion: {emotion}, Action: {action}");
        OnEmotionAction?.Invoke(emotion, action);
    }

    [System.Serializable]
    public class KeyEmotionBinding
    {
        public KeyCode key;
        public Emotion emotion;

        public KeyEmotionBinding(KeyCode key, Emotion emotion)
        {
            this.key = key;
            this.emotion = emotion;
        }
    }

    [System.Serializable]
    public class KeyActionBinding
    {
        public KeyCode key;
        public Behavior action;

        public KeyActionBinding(KeyCode key, Behavior action)
        {
            this.key = key;
            this.action = action;
        }
    }
}