using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptik.Game
{
    [CreateAssetMenu(menuName = "Synaptik/Alien/Reaction Matrix", fileName = "ReactionMatrix")]
    public class ReactionMatrix : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("_interrationRules")] private InterractionRule[] _interactionRules = Array.Empty<InterractionRule>();
        [SerializeField] private ItemRule[] _itemRules = Array.Empty<ItemRule>();

        private Dictionary<InteractionKey, List<InterractionRule>> _interactionLookup;
        private Dictionary<string, ItemRule> _itemLookup;
        private int _cachedInteractionRuleCount;
        private int _cachedItemRuleCount;
        private int _cachedInteractionRuleHash;

        private void OnEnable()
        {
            BuildLookups();
        }

        private void OnValidate()
        {
            BuildLookups();
        }

        public bool TryFindRule(Behavior channel, Emotion playerEmotion, Func<InterractionRule, bool> predicate, out InterractionRule rule)
        {
            EnsureInteractionLookup();
            if (!_interactionLookup.TryGetValue(new InteractionKey(channel, playerEmotion), out var candidates) || candidates.Count == 0)
            {
                rule = default;
                return false;
            }

            if (predicate != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (predicate(candidates[i]))
                    {
                        rule = candidates[i];
                        return true;
                    }
                }

                rule = default;
                return false;
            }

            rule = candidates[candidates.Count - 1];
            return true;
        }

        public bool TryFindItemRule(string itemId, out ItemRule rule)
        {
            EnsureItemLookup();
            if (string.IsNullOrEmpty(itemId))
            {
                rule = default;
                return false;
            }

            return _itemLookup.TryGetValue(itemId, out rule);
        }

        private void EnsureInteractionLookup()
        {
            if (_interactionLookup == null)
            {
                BuildInteractionLookup();
                return;
            }

            if (_cachedInteractionRuleCount != _interactionRules.Length)
            {
                BuildInteractionLookup();
                return;
            }

            if (_cachedInteractionRuleHash != ComputeInteractionRulesHash())
            {
                BuildInteractionLookup();
            }
        }

        private void EnsureItemLookup()
        {
            if (_itemLookup == null || _cachedItemRuleCount != _itemRules.Length)
            {
                BuildItemLookup();
            }
        }

        private void BuildLookups()
        {
            BuildInteractionLookup();
            BuildItemLookup();
        }

        private void BuildInteractionLookup()
        {
            var lookup = new Dictionary<InteractionKey, List<InterractionRule>>(_interactionRules.Length);

            for (int i = 0; i < _interactionRules.Length; i++)
            {
                var key = new InteractionKey(_interactionRules[i].Channel, _interactionRules[i].PlayerEmotion);
                if (!lookup.TryGetValue(key, out var rules))
                {
                    rules = new List<InterractionRule>();
                    lookup.Add(key, rules);
                }

                rules.Add(_interactionRules[i]);
            }

            _interactionLookup = lookup;
            _cachedInteractionRuleCount = _interactionRules.Length;
            _cachedInteractionRuleHash = ComputeInteractionRulesHash();
        }

        private void BuildItemLookup()
        {
            var lookup = new Dictionary<string, ItemRule>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _itemRules.Length; i++)
            {
                var expectedItemId = _itemRules[i].ExpectedItemId;
                if (string.IsNullOrEmpty(expectedItemId))
                {
                    continue;
                }

                if (lookup.ContainsKey(expectedItemId))
                {
                    Debug.LogWarning($"Duplicate item rule detected for '{expectedItemId}'. Only the first occurrence will be used.");
                    continue;
                }

                lookup.Add(expectedItemId, _itemRules[i]);
            }

            _itemLookup = lookup;
            _cachedItemRuleCount = _itemRules.Length;
        }

        private int ComputeInteractionRulesHash()
        {
            var hashCode = new HashCode();

            for (int i = 0; i < _interactionRules.Length; i++)
            {
                var rule = _interactionRules[i];
                hashCode.Add(rule.Channel);
                hashCode.Add(rule.PlayerEmotion);
                hashCode.Add(rule.QuestId);
                hashCode.Add(rule.QuestStepId);
                hashCode.Add(rule.SuspicionDelta);
                hashCode.Add(rule.SetNewEmotion);
                hashCode.Add(rule.NewEmotion);
            }

            return hashCode.ToHashCode();
        }

        private readonly struct InteractionKey : IEquatable<InteractionKey>
        {
            private readonly Behavior _behavior;
            private readonly Emotion _emotion;

            public InteractionKey(Behavior behavior, Emotion emotion)
            {
                _behavior = behavior;
                _emotion = emotion;
            }

            public bool Equals(InteractionKey other)
            {
                return _behavior == other._behavior && _emotion == other._emotion;
            }

            public override bool Equals(object obj)
            {
                return obj is InteractionKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)_behavior * 397) ^ (int)_emotion;
                }
            }

            public override string ToString()
            {
                return $"{_behavior}/{_emotion}";
            }
        }
    }
}
