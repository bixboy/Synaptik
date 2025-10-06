using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

    public List<KeyMovementBinding> movementBindings = new List<KeyMovementBinding>()
    {
        new KeyMovementBinding(KeyCode.DownArrow, Vector2.down),
        new KeyMovementBinding(KeyCode.UpArrow, Vector2.up),
        new KeyMovementBinding(KeyCode.LeftArrow, Vector2.left),
        new KeyMovementBinding(KeyCode.RightArrow, Vector2.right)
    };

    public delegate void EmotionActionDelegate(Emotion emotion, Behavior action);
    public event EmotionActionDelegate OnEmotionAction;

    private Dictionary<KeyCode, Emotion> _emotionMap;
    private Dictionary<KeyCode, Behavior> _actionMap;
    private Dictionary<KeyCode, Vector2> _movementMap;

    private HashSet<KeyCode> _pressedEmotionKeys = new HashSet<KeyCode>();
    private HashSet<KeyCode> _pressedActionKeys = new HashSet<KeyCode>();
    private HashSet<KeyCode> _pressedMovementKeys = new HashSet<KeyCode>();

    public Vector2 MoveVector
    {
        get
        {
            Vector2 moveVector = Vector2.zero;
            
            foreach (KeyCode key in _pressedMovementKeys)
            {
                if (_movementMap.TryGetValue(key, out Vector2 dir))
                    moveVector += dir;
            }

            return moveVector.normalized;
        }
    }
    
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
        
        foreach (var a in movementBindings)
            if (!_movementMap.ContainsKey(a.key))
                _movementMap.Add(a.key, a.direction);

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

        foreach (var kvp in _movementMap)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                _pressedMovementKeys.Add(kvp.Key);
            }

            if (Input.GetKeyUp(kvp.Key))
            {
                _pressedMovementKeys.Remove(kvp.Key);
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

    [System.Serializable]
    public class KeyMovementBinding
    {
        public KeyCode key;
        public Vector2 direction;

        public KeyMovementBinding(KeyCode key, Vector2 dir)
        {
            this.key = key;
            this.direction = dir.normalized;
        }
    }
}
