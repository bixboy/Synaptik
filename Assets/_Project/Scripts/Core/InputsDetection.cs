using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système de détection combinée : émotion + action.
/// Quand une touche « émotion » et une touche « action » sont pressées simultanément,
/// un callback global est déclenché.
/// </summary>
public sealed class InputsDetection : MonoBehaviour
{
    public static InputsDetection Instance { get; private set; }

    [Header("Configuration des touches ↔ émotions")]
    public List<KeyEmotionBinding> emotionBindings = new()
    {
        new KeyEmotionBinding(KeyCode.F1, Emotion.Anger),
        new KeyEmotionBinding(KeyCode.F2, Emotion.Curious),
        new KeyEmotionBinding(KeyCode.F3, Emotion.Fearful),
        new KeyEmotionBinding(KeyCode.F4, Emotion.Friendly)
    };

    [Header("Configuration des touches ↔ actions")]
    public List<KeyActionBinding> actionBindings = new()
    {
        new KeyActionBinding(KeyCode.F5, Behavior.Talking),
        new KeyActionBinding(KeyCode.F6, Behavior.Action)
    };

    [Header("Configuration des touches ↔ mouvement")]
    public List<KeyMovementBinding> movementBindings = new()
    {
        new KeyMovementBinding(KeyCode.UpArrow,    Vector2.up),
        new KeyMovementBinding(KeyCode.DownArrow,  Vector2.down),
        new KeyMovementBinding(KeyCode.LeftArrow,  Vector2.left),
        new KeyMovementBinding(KeyCode.RightArrow, Vector2.right),
        new KeyMovementBinding(KeyCode.W, Vector2.up),
        new KeyMovementBinding(KeyCode.S, Vector2.down),
        new KeyMovementBinding(KeyCode.A, Vector2.left),
        new KeyMovementBinding(KeyCode.D, Vector2.right)
    };

    private readonly HashSet<KeyCode> _pressedEmotionKeys = new();
    private readonly HashSet<KeyCode> _pressedActionKeys = new();

    private readonly Dictionary<KeyCode, Emotion> _emotionMap = new();
    private readonly Dictionary<KeyCode, Behavior> _actionMap = new();

    private KeyCode[] _moveKeys = Array.Empty<KeyCode>();
    private Vector2[] _moveDirs = Array.Empty<Vector2>();

    [SerializeField]
    private Vector2 _moveVector;

    private int _actionInputPressed;

    public Vector2 MoveVector => _moveVector;

    public event Action<Emotion, Behavior> OnEmotionAction;
    public event Action<Emotion, bool> OnEmotion;
    public event Action<Behavior, bool> OnAction;
    public event Action<bool> OnTowActionPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _moveKeys = new KeyCode[movementBindings.Count];
        _moveDirs = new Vector2[movementBindings.Count];

        foreach (var binding in emotionBindings)
        {
            if (_emotionMap.ContainsKey(binding.key))
            {
                continue;
            }

            _emotionMap.Add(binding.key, binding.emotion);
        }

        foreach (var binding in actionBindings)
        {
            if (_actionMap.ContainsKey(binding.key))
            {
                continue;
            }

            _actionMap.Add(binding.key, binding.action);
        }

        for (var i = 0; i < movementBindings.Count; i++)
        {
            _moveKeys[i] = movementBindings[i].key;
            _moveDirs[i] = movementBindings[i].direction;
        }

        Debug.Log($"[InputsDetection] {_emotionMap.Count} émotions / {_actionMap.Count} actions configurées.");
    }

    private void Update()
    {
        UpdateMovement();
        UpdateEmotionInputs();
        UpdateActionInputs();
    }

    private void UpdateMovement()
    {
        var sum = Vector2.zero;
        for (var i = 0; i < _moveKeys.Length; i++)
        {
            if (Input.GetKey(_moveKeys[i]))
            {
                sum += _moveDirs[i];
            }
        }

        if (sum.sqrMagnitude > 1f)
        {
            sum.Normalize();
        }

        _moveVector = sum;
    }

    private void UpdateEmotionInputs()
    {
        foreach (var (keyCode, emotion) in _emotionMap)
        {
            if (Input.GetKeyDown(keyCode))
            {
                _pressedEmotionKeys.Add(keyCode);
                TryTriggerCombos(emotion);
                OnEmotion?.Invoke(emotion, false);
            }

            if (Input.GetKeyUp(keyCode))
            {
                _pressedEmotionKeys.Remove(keyCode);
                OnEmotion?.Invoke(emotion, true);
            }
        }
    }

    private void UpdateActionInputs()
    {
        foreach (var (keyCode, action) in _actionMap)
        {
            if (Input.GetKeyDown(keyCode))
            {
                _pressedActionKeys.Add(keyCode);
                TryTriggerCombos(action: action);
                OnAction?.Invoke(action, false);

                _actionInputPressed++;
                if (_actionInputPressed >= 2)
                {
                    OnTowActionPressed?.Invoke(true);
                }
            }

            if (Input.GetKeyUp(keyCode))
            {
                _pressedActionKeys.Remove(keyCode);
                _actionInputPressed = Mathf.Max(0, _actionInputPressed - 1);
                OnTowActionPressed?.Invoke(false);
                OnAction?.Invoke(action, true);
            }
        }
    }

    private void TryTriggerCombos(Emotion emotion = Emotion.None, Behavior action = Behavior.None)
    {
        if (emotion != Emotion.None)
        {
            foreach (var key in _pressedActionKeys)
            {
                var activeAction = _actionMap[key];
                Trigger(emotion, activeAction);
            }
        }

        if (action == Behavior.None)
        {
            return;
        }

        foreach (var key in _pressedEmotionKeys)
        {
            var activeEmotion = _emotionMap[key];
            Trigger(activeEmotion, action);
        }
    }

    private void Trigger(Emotion emotion, Behavior action)
    {
        OnEmotionAction?.Invoke(emotion, action);
    }

    [Serializable]
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

    [Serializable]
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

    [Serializable]
    public class KeyMovementBinding
    {
        public KeyCode key;
        public Vector2 direction;

        public KeyMovementBinding(KeyCode key, Vector2 dir)
        {
            this.key = key;
            direction = dir.normalized;
        }
    }
}
