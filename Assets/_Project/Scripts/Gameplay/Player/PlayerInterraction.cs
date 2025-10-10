using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptik.Game
{
    public class PlayerInteraction : MonoBehaviour
    {
        
        [Header("Pickup/Drop Settings")]
        [SerializeField] private Transform _handSocket;         // vide placé dans la main
        [SerializeField] private float _pickupRadius = 1.2f;    // portée
        [SerializeField] private LayerMask _pickupMask = ~0;    // couche(s) des objets
        [SerializeField] private float _dropForwardSpeed = 0f;  // 0 = lâcher sans lancer

        private HoldableItem _held;
        private string _heldItemId;
        
        [Header("Interaction Settings")] 
        [SerializeField] private Transform _aimZone;
        [SerializeField] private float _interactRadius = 2f;
        [SerializeField] private float _interactHalfFov = 45f;
        [SerializeField] private LayerMask _interactMask;

        private static readonly Collider[] _overlap = new Collider[64];
        
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
        [Header("Player Feedback")]
        [SerializeField] private PlayerInteractionFeedback _interactionFeedback;
        
        
        private void Awake()
        {
            _comboBubble = GetComponent<PlayerComboBubble>();
            if (_comboBubble == null)
            {
                _comboBubble = gameObject.AddComponent<PlayerComboBubble>();
            }

            if (_interactionFeedback == null)
            {
                _interactionFeedback = GetComponent<PlayerInteractionFeedback>();
                if (_interactionFeedback == null)
                {
                    _interactionFeedback = gameObject.AddComponent<PlayerInteractionFeedback>();
                }
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
            IInteraction interactable = TargetingUtil.FindInteractionInFront(origin, _interactRadius, _interactHalfFov, _interactMask);

            if (interactable != null)
            {
                interactable?.Interact(new ActionValues(emotion, behavior), _held, this);
            }
            else if (emotion == Emotion.Friendly && behavior == Behavior.Action && _held)
            {
                DropItem();
            }
            else if (emotion == Emotion.Friendly && behavior == Behavior.Action && !_held)
            {
                _interactionFeedback?.ShowFriendlyWithoutItem();
            }
            else
            {
                _interactionFeedback?.ShowComboNoTarget(emotion, behavior, _held != null);
            }
        }

        
        public void PickUp()
        {
            Vector3 origin = _handSocket ? _handSocket.position : transform.position;
            int count = Physics.OverlapSphereNonAlloc(origin, _pickupRadius, _overlap, _pickupMask, QueryTriggerInteraction.Ignore);
            if (count <= 0)
            {
                if (_held == null)
                {
                    _interactionFeedback?.ShowPickupUnavailable();
                }
                else
                {
                    Debug.Log("PickUp: aucun objet à portée");
                }
                return;
            }

            HoldableItem best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _overlap[i];
                if (!col || !col.gameObject.activeInHierarchy) continue;

                var holdable = col.GetComponentInParent<HoldableItem>();
                if (!holdable) continue;

                // ignore l'objet déjà en main ou momentanément non-prenable
                if ((_held && holdable == _held) || !holdable.CanBePicked) continue;

                float sqr = (holdable.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = holdable; }
            }

            if (!best)
            {
                if (_held == null)
                {
                    _interactionFeedback?.ShowPickupUnavailable();
                }
                else
                {
                    Debug.Log("PickUp: aucun objet à portée");
                }
                return;
            }

            // swap si on tient déjà quelque chose
            string previousItemName = GetItemDisplayName(_held);
            if (_held)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
            }

            best.Pick(_handSocket ? _handSocket : transform);
            _held = best;
            _heldItemId = _held.ItemId;

            string newItemName = GetItemDisplayName(_held);
            if (!string.IsNullOrWhiteSpace(previousItemName))
            {
                _interactionFeedback?.ShowPickupSwap(previousItemName, newItemName);
            }
            else
            {
                _interactionFeedback?.ShowPickupSuccess(newItemName);
            }
        }

        public void DropItem(bool destroyItem = false)
        {
            if (!_held)
            {
                _interactionFeedback?.ShowNoItemToDrop();
                return;
            }

            string heldItemName = GetItemDisplayName(_held);

            if (destroyItem)
            {
                Destroy(_held.gameObject);
                _held = null;
                _heldItemId = null; // si tu utilises un ID
                _interactionFeedback?.ShowItemConsumed(heldItemName);
                return;
            }

            // tenter un don à un alien (sinon drop au sol)
            Transform origin = _aimZone ? _aimZone : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _interactMask);

            bool hasAlien = alien != null;
            bool withinRadius = hasAlien && alien.IsWithinReceiveRadius(origin.position);
            string alienName = GetAlienDisplayName(alien);

            bool didGive = false;
            if (withinRadius)
            {
                // l'alien accepte ?
                didGive = alien.TryReceiveItem(_heldItemId);
            }

            if (didGive)
            {
                // consommé par l'alien
                Destroy(_held.gameObject);
                _held = null;
                _heldItemId = null;
                _interactionFeedback?.ShowGiftAccepted(heldItemName, alienName);
                return;
            }

            // drop “au sol”
            Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
            _held.Drop(v);
            _held = null;
            _heldItemId = null;

            if (hasAlien)
            {
                if (!withinRadius)
                {
                    _interactionFeedback?.ShowNeedToApproach(alienName);
                }
                else
                {
                    _interactionFeedback?.ShowGiftRefused(heldItemName, alienName);
                }
            }
            else
            {
                _interactionFeedback?.ShowDropOnGround(heldItemName);
            }
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

        private static string GetItemDisplayName(HoldableItem item)
        {
            if (!item)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(item.ItemId))
            {
                return item.ItemId;
            }

            return item.gameObject.name;
        }

        private static string GetAlienDisplayName(Alien alien)
        {
            if (!alien)
            {
                return string.Empty;
            }

            if (alien.Definition != null)
            {
                return alien.Definition.name;
            }

            return alien.name;
        }
    }
}
