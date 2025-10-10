using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptik.Game
{
    public class PlayerInteraction : MonoBehaviour
    {
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

        [Header("Combo Feedback")]
        [SerializeField] private float _defaultComboBubbleDuration = 1.75f;
        [SerializeField] private ComboSymbolDefinition[] _comboSymbolDefinitions =
        {
            new ComboSymbolDefinition(Emotion.Anger,    Behavior.Talking, "💬⚡", 2f),
            new ComboSymbolDefinition(Emotion.Friendly, Behavior.Talking, "💬❤️", 2f),
            new ComboSymbolDefinition(Emotion.Curious,  Behavior.Talking, "💬❓", 2f),
            new ComboSymbolDefinition(Emotion.Fearful,  Behavior.Talking, "💬😱", 2f),
            new ComboSymbolDefinition(Emotion.Anger,    Behavior.Action,  "✋⚡", 1.75f),
            new ComboSymbolDefinition(Emotion.Friendly, Behavior.Action,  "✋❤️", 1.75f),
            new ComboSymbolDefinition(Emotion.Curious,  Behavior.Action,  "✋❓", 1.75f),
            new ComboSymbolDefinition(Emotion.Fearful,  Behavior.Action,  "✋😱", 1.75f),
        };

        private readonly Dictionary<ComboKey, ComboSymbolDefinition> _comboLookup = new();
        private PlayerComboBubble _comboBubble;

        [Header("Pickup/Drop Settings")]
        [SerializeField] private Transform _handSocket;         // vide placé dans la main
        [SerializeField] private float _pickupRadius = 1.2f;    // portée
        [SerializeField] private LayerMask _pickupMask = ~0;    // couche(s) des objets
        [SerializeField] private float _dropForwardSpeed = 0f;  // 0 = lâcher sans lancer

        private HoldableItem _held;
        private string _heldItemId => _held != null ? _held.ItemId : null;

        
        [Header("Interaction Settings")] 
        [SerializeField] private Transform _aimZone;
        [SerializeField] private float _interactRadius = 2f;
        [SerializeField] private float _interactHalfFov = 45f;
        [SerializeField] private LayerMask _alienMask;

        private void Awake()
        {
            _comboBubble = GetComponent<PlayerComboBubble>();
            if (_comboBubble == null)
            {
                _comboBubble = gameObject.AddComponent<PlayerComboBubble>();
            }

            RebuildComboLookup();
        }

        private void Start()
        {
            InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
        }

        private void OnEnable()
        {
            if (InputsDetection.Instance != null)
            {
                InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
            }
        }

        private void OnDisable()
        {
            if (InputsDetection.Instance != null)
            {
                InputsDetection.Instance.OnEmotionAction -= HandleEmotionAction;
            }
        }

        private void OnValidate()
        {
            RebuildComboLookup();
        }

        private void RebuildComboLookup()
        {
            _comboLookup.Clear();
            if (_comboSymbolDefinitions == null)
            {
                return;
            }

            foreach (var definition in _comboSymbolDefinitions)
            {
                if (definition.Behavior == Behavior.None || definition.Emotion == Emotion.None)
                {
                    continue;
                }

                var key = new ComboKey(definition.Emotion, definition.Behavior);
                if (_comboLookup.ContainsKey(key))
                {
                    _comboLookup[key] = definition;
                }
                else
                {
                    _comboLookup.Add(key, definition);
                }
            }
        }

        private void ShowComboFeedback(Emotion emotion, Behavior behavior)
        {
            if (_comboBubble == null || emotion == Emotion.None || behavior == Behavior.None)
            {
                return;
            }

            if (_comboLookup.Count == 0)
            {
                RebuildComboLookup();
            }

            var key = new ComboKey(emotion, behavior);
            if (_comboLookup.TryGetValue(key, out var definition) && !string.IsNullOrWhiteSpace(definition.Symbols))
            {
                var duration = definition.Duration > 0f ? definition.Duration : _defaultComboBubbleDuration;
                _comboBubble.Show(definition.Symbols, duration);
                return;
            }

            if (!DefaultBehaviorSymbols.TryGetValue(behavior, out var behaviorSymbol) ||
                !DefaultEmotionSymbols.TryGetValue(emotion, out var emotionSymbol))
            {
                return;
            }

            _comboBubble.Show(behaviorSymbol + emotionSymbol, _defaultComboBubbleDuration);
        }

        private void HandleEmotionAction(Emotion emotion, Behavior behavior)
        {
            ShowComboFeedback(emotion, behavior);
            Debug.Log($"HandleEmotionAction: {emotion} + {behavior}");
            Transform origin = _aimZone != null ? _aimZone : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _alienMask);
            if (alien == null)
            {
                if (!_held && emotion == Emotion.Curious && behavior == Behavior.Action) PickUp();
                else if (_held && emotion == Emotion.Friendly && behavior == Behavior.Action) DropItem();
                return;
            }

            if (behavior == Behavior.Action)
            {
                switch (emotion)
                {
                    case Emotion.Anger: // Hit
                        break;
                    case Emotion.Curious: // Ramasser
                    {
                        PickUp();
                        break;
                    }
                        
                    case Emotion.Fearful: // Courir
                        break;
                    case Emotion.Friendly: // Donne
                    {
                        if (alien.Definition.Reactions.TryFindItemRule(_heldItemId, out var itemRule))
                        {
                            alien.TryReceiveItem(_heldItemId);
                            DropItem(true);
                            Debug.Log($"Give item {itemRule.ExpectedItemId} to alien {alien.Definition.name}");
                            return;
                        }
                        DropItem();
                        
                        Debug.Log("Drop item in front of alien");
                        return;
                    }
                        
                }
            }
            else if (behavior == Behavior.Talking)
            {
                switch (emotion)
                {
                    case Emotion.Anger: // Insulter
                        break;
                    case Emotion.Curious: // Curieux
                        break;
                    case Emotion.Fearful: // Crie
                        break;
                    case Emotion.Friendly: // Complimenter 
                        break;
                }
            }
            
            
            alien.OnPlayerCombo(emotion, behavior);
        }

        public void PickUp()
        {
            Debug.Log("PickUp: tenter de ramasser un objet");
            Vector3 origin = _handSocket ? _handSocket.position : transform.position;
            Collider[] hits = Physics.OverlapSphere(origin, _pickupRadius, _pickupMask, QueryTriggerInteraction.Ignore);

            HoldableItem best = null;
            float bestDist = float.MaxValue;

            foreach (var h in hits)
            {
                var holdable = h.GetComponentInParent<HoldableItem>();
                if (!holdable) 
                    continue;


                if (_held && holdable == _held)
                    continue;

                float d = Vector3.SqrMagnitude(h.transform.position - origin);
                if (d < bestDist) { bestDist = d; best = holdable; }
            }
            
            if (!best)
            {
                if (!_held)
                    Debug.Log("PickUp: aucun objet à portée");
                
                return;
            }
            
            if (_held)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
            }
            
            best.Pick(_handSocket ? _handSocket : transform);
            _held = best;
        }

        public void DropItem(bool destroyItem = false)
        {
            if (!_held)
                return;
            
            if (destroyItem)
            {
                Destroy(_held.gameObject);
                _held = null;
                
                return;
            }
            Transform origin = _aimZone ? _aimZone : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _alienMask);

            if (!alien)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
            }
            else
            {
                if (alien.IsWithinReceiveRadius(origin.position))
                {
                    if (alien.TryReceiveItem(_heldItemId));
                    else
                    {
                        Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                        _held.Drop(v);
                    }
                }
            }
            _held = null;
        }

        public void OnDrawGizmos()
        {
            if (_handSocket != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_handSocket.position, _pickupRadius);
            }

            if (_aimZone != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_aimZone.position, _interactRadius);
                Gizmos.DrawLine(_aimZone.position, _aimZone.position + Quaternion.Euler(0f, _interactHalfFov, 0f) * _aimZone.forward * _interactRadius);
                Gizmos.DrawLine(_aimZone.position, _aimZone.position + Quaternion.Euler(0f, -_interactHalfFov, 0f) * _aimZone.forward * _interactRadius);
            }
        }
    }
}