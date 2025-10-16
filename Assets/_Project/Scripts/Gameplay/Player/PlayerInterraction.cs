using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private const string LogPrefix = "[PlayerInteraction]";

    [Header("Pickup/Drop Settings")]
    [SerializeField]
    private Transform handSocket;

    [SerializeField]
    private float pickupRadius = 1.2f;

    [SerializeField]
    private LayerMask pickupMask = ~0;

    [SerializeField]
    private float dropForwardSpeed;

    private HoldableItem heldItem;
    private string heldItemId;

    [Header("Interaction Settings")]
    [SerializeField]
    private Transform aimZone;

    [SerializeField]
    private float interactRadius = 2f;

    [SerializeField]
    private float interactHalfFov = 45f;

    [SerializeField]
    private LayerMask interactMask;

    [Header("Combo Feedback")]
    [SerializeField]
    private float defaultComboBubbleDuration = 1.75f;
    

    [SerializeField]
    private ComboSymbolDefinition[] comboSymbolDefinitions =
    {
        new ComboSymbolDefinition(Emotion.Anger,    Behavior.Talking, "💬⚡", 2f),
        new ComboSymbolDefinition(Emotion.Friendly, Behavior.Talking, "💬❤️", 2f),
        new ComboSymbolDefinition(Emotion.Curious,  Behavior.Talking, "💬❓", 2f),
        new ComboSymbolDefinition(Emotion.Fearful,  Behavior.Talking, "💬😱", 2f),
        new ComboSymbolDefinition(Emotion.Anger,    Behavior.Action,  "✋⚡", 1.75f),
        new ComboSymbolDefinition(Emotion.Friendly, Behavior.Action,  "✋❤️", 1.75f),
        new ComboSymbolDefinition(Emotion.Curious,  Behavior.Action,  "✋❓", 1.75f),
        new ComboSymbolDefinition(Emotion.Fearful,  Behavior.Action,  "✋😱", 1.75f)
    };

    private readonly Dictionary<ComboKey, ComboSymbolDefinition> comboLookup = new();
    private PlayerComboBubble comboBubble;
    private bool isInInteractionZone;

    private static readonly Collider[] overlap = new Collider[64];

    [Serializable]
    private struct ComboSymbolDefinition
    {
        public Emotion Emotion;
        public Behavior Behavior;
        public string Symbols;
        public float Duration;

        public ComboSymbolDefinition(Emotion emotion, Behavior behavior, string symbols, float duration)
        {
            Emotion = emotion;
            Behavior = behavior;
            Symbols = symbols;
            Duration = duration;
        }
    }

    private readonly struct ComboKey : IEquatable<ComboKey>
    {
        public readonly Emotion Emotion;
        public readonly Behavior Behavior;

        public ComboKey(Emotion emotion, Behavior behavior)
        {
            Emotion = emotion;
            Behavior = behavior;
        }

        public bool Equals(ComboKey other)
        {
            return Emotion == other.Emotion && Behavior == other.Behavior;
        }

        public override bool Equals(object obj)
        {
            return obj is ComboKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Emotion * 397) ^ (int)Behavior;
            }
        }
    }

    private static readonly Dictionary<Emotion, string> DefaultEmotionSymbols = new()
    {
        { Emotion.Anger, "⚡" },
        { Emotion.Friendly, "❤️" },
        { Emotion.Curious, "❓" },
        { Emotion.Fearful, "😱" }
    };

    private static readonly Dictionary<Behavior, string> DefaultBehaviorSymbols = new()
    {
        { Behavior.Talking, "💬" },
        { Behavior.Action, "✋" }
    };
    
    
    [Header("Player Animation")]
    [SerializeField] private PlayerAnimation _playerAnimation;
    
    
    [Header("Angry Zone")]
    public bool IsInAngryZone;
    

    private void Reset()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
        if (!_playerAnimation) Debug.LogWarning("PlayerInteraction: pas de PlayerAnimation assigné !", this);
    }
    public event Action<bool> InteractionZoneChanged;

    private void Awake()
    {
        comboBubble = GetComponent<PlayerComboBubble>() ?? gameObject.AddComponent<PlayerComboBubble>();
        if (!_playerAnimation)
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            if (!_playerAnimation)
                Debug.LogWarning("PlayerInteraction: pas de PlayerAnimation assigné !", this);
        }
        RebuildComboLookup();
        Debug.Log($"{LogPrefix} '{name}' prêt ({comboLookup.Count} combos).");
    }

    private void Start()
    {
        if (InputsDetection.Instance)
        {
            InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
            InputsDetection.Instance.OnEmotion += HandleEmotion;
            Debug.Log($"{LogPrefix} Abonné aux combos d'InputsDetection.");
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} Aucun InputsDetection trouvé lors de l'initialisation.");
        }
    }

    private void OnDestroy()
    {
        if (InputsDetection.Instance)
        {
            InputsDetection.Instance.OnEmotionAction -= HandleEmotionAction;
            InputsDetection.Instance.OnEmotion -= HandleEmotion;
            Debug.Log($"{LogPrefix} Désabonné des combos d'InputsDetection.");
        }
    }

    private void OnValidate()
    {
        RebuildComboLookup();
    }

    private void Update()
    {
        UpdateInteractionZoneState();
    }

    private void RebuildComboLookup()
    {
        comboLookup.Clear();
        if (comboSymbolDefinitions == null)
        {
            Debug.LogWarning($"{LogPrefix} Aucun symbole de combo configuré.");
            return;
        }

        foreach (var definition in comboSymbolDefinitions)
        {
            if (definition.Behavior == Behavior.None || definition.Emotion == Emotion.None)
                continue;

            var key = new ComboKey(definition.Emotion, definition.Behavior);
            comboLookup[key] = definition;
        }

        Debug.Log($"{LogPrefix} Table de combos reconstruite ({comboLookup.Count} entrées).");
    }
    
    private void HandleEmotion(Emotion emotion, bool keyReleased)
    {
        if (!keyReleased)
        {
            _playerAnimation?.SetEmotion(emotion);
        }
        else
        {
            _playerAnimation?.UnsetEmotion(emotion);
        }
    }
    private void HandleEmotionAction(Emotion emotion, Behavior behavior)
    {
        ShowComboFeedback(emotion, behavior);
        

        if (TryFindInteractionTarget(out var interactable))
        {
            Debug.Log($"{LogPrefix} Combo {emotion}/{behavior} → interactable '{interactable}'.");
            interactable.Interact(new ActionValues(emotion, behavior), heldItem, this);
        }
        else if (emotion == Emotion.Friendly && behavior == Behavior.Action && heldItem)
        {
            DropItem();
        }
        
        if (emotion == Emotion.Anger)
        {
            if (behavior == Behavior.Action)
                _playerAnimation?.PlayPunch();
            else if (behavior == Behavior.Talking && IsInAngryZone)
                Debug.Log("Quête de l'Angry Zone déclenchée !");
        }
    }

    public void PickUp()
    {
        var origin = handSocket ? handSocket.position : transform.position;
        var count = Physics.OverlapSphereNonAlloc(origin, pickupRadius, overlap, pickupMask, QueryTriggerInteraction.Ignore);
        if (count <= 0)
        {
            return;
        }

        HoldableItem bestCandidate = null;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < count; i++)
        {
            var collider = overlap[i];
            if (!collider || !collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            var holdable = collider.GetComponentInParent<HoldableItem>();
            if (!holdable || !holdable.CanBePicked || holdable == heldItem)
            {
                continue;
            }

            var distance = (holdable.transform.position - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCandidate = holdable;
            }
        }

        if (!bestCandidate)
            return;

        if (heldItem)
        {
            var velocity = dropForwardSpeed > 0f ? transform.forward * dropForwardSpeed : Vector3.zero;
            heldItem.Drop(velocity);
            heldItem = null;
        }

        bestCandidate.Pick(handSocket ? handSocket : transform);
        heldItem = bestCandidate;
        heldItemId = heldItem.ItemId;
        _playerAnimation?.OnPickedUpItem();
        Debug.Log($"{LogPrefix} Objet '{heldItem.name}' ramassé (ID: {heldItemId}).");
    }

    public void DropItem(bool destroyItem = false)
    {
        if (!heldItem)
        {
            Debug.Log($"{LogPrefix} Aucun objet à déposer.");
            return;
        }

        if (destroyItem)
        {
            Destroy(heldItem.gameObject);
            Debug.Log($"{LogPrefix} Objet '{heldItemId}' détruit.");
            
            heldItem = null;
            heldItemId = null;
            
            return;
        }

        var origin = aimZone ? aimZone : transform;
        var alien = TargetingUtil.FindAlienInFront(origin, interactRadius, interactHalfFov, interactMask);

        var gaveItem = false;
        if (alien && alien.IsWithinReceiveRadius(origin.position))
        {
            gaveItem = alien.TryReceiveItem(heldItemId);
            Debug.Log($"{LogPrefix} Don de '{heldItemId}' à '{alien.name}' → succès={gaveItem}.");
        }

        if (gaveItem)
        {
            Destroy(heldItem.gameObject);
        }
        else
        {
            var velocity = dropForwardSpeed > 0f ? transform.forward * dropForwardSpeed : Vector3.zero;
            heldItem.Drop(velocity);
        }

        heldItem = null;
        heldItemId = null;
        _playerAnimation?.OnDroppedItem();
    }

    public void OnDrawGizmos()
    {
        if (handSocket != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handSocket.position, pickupRadius);
        }

        if (aimZone != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(aimZone.position, interactRadius);
            Gizmos.DrawLine(aimZone.position, aimZone.position + Quaternion.Euler(0f, interactHalfFov, 0f) * aimZone.forward * interactRadius);
            Gizmos.DrawLine(aimZone.position, aimZone.position + Quaternion.Euler(0f, -interactHalfFov, 0f) * aimZone.forward * interactRadius);
        }
    }

    private void ShowComboFeedback(Emotion emotion, Behavior behavior)
    {
        if (!comboBubble || emotion == Emotion.None || behavior == Behavior.None)
        {
            return;
        }

        if (comboLookup.Count == 0)
        {
            RebuildComboLookup();
            if (comboLookup.Count == 0)
            {
                Debug.LogWarning($"{LogPrefix} Aucun combo disponible pour l'affichage de feedback.");
            }
        }

        var key = new ComboKey(emotion, behavior);
        if (comboLookup.TryGetValue(key, out var definition) && !string.IsNullOrWhiteSpace(definition.Symbols))
        {
            var duration = definition.Duration > 0f ? definition.Duration : defaultComboBubbleDuration;
            comboBubble.Show(definition.Emotion, definition.Symbols, duration);

            return;
        }

        if (DefaultBehaviorSymbols.TryGetValue(behavior, out var behaviorSymbol) &&
            DefaultEmotionSymbols.TryGetValue(emotion, out var emotionSymbol))
        {
            comboBubble.Show(emotion, behaviorSymbol + emotionSymbol, defaultComboBubbleDuration);
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} Impossible de trouver un feedback pour le combo {behavior}/{emotion}.");
        }
    }

    private bool TryFindInteractionTarget(out IInteraction interaction)
    {
        var origin = aimZone ? aimZone : transform;
        interaction = TargetingUtil.FindInteractionInFront(origin, interactRadius, interactHalfFov, interactMask);
        return interaction != null;
    }

    private void UpdateInteractionZoneState()
    {
        var hasInteraction = TryFindInteractionTarget(out _);
        if (hasInteraction == isInInteractionZone)
        {
            return;
        }

        isInInteractionZone = hasInteraction;
        InteractionZoneChanged?.Invoke(isInInteractionZone);
    }
}
