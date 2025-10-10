using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptik.Game
{
    [RequireComponent(typeof(Animator))]
    public class Alien : MonoBehaviour
    {
        [SerializeField] private AlienDefinition _def;
        public AlienDefinition Definition => _def;
        [SerializeField] private float _receiveRadius = 1.3f;
        [FormerlySerializedAs("_dialogueBubblePrefab")]
        [SerializeField] private DialogueBubble _dialogueBubble;

        public Emotion Emotion { get; private set; }

        private Animator _anim;
        private static readonly int EmotionHash = Animator.StringToHash("Emotion");

        private readonly Dictionary<string, AlienQuestRuntime> _questRuntimes = new Dictionary<string, AlienQuestRuntime>();
        private readonly Dictionary<string, int> _receivedItemQuantities = new Dictionary<string, int>();

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            if (_def != null && _def.Animator != null)
            {
                _anim.runtimeAnimatorController = _def.Animator;
            }

            Emotion = _def != null ? _def.StartEmotion : Emotion.Curious;
            ApplyAnimFromEmotion();
        }

        private void Start()
        {
            if (AlienManager.Instance != null)
            {
                AlienManager.Instance.RegisterAlien(this);
            }

            if (_def == null)
            {
                return;
            }

            foreach (AlienQuest quest in _def.Quests)
            {
                var hasQuestId = !string.IsNullOrWhiteSpace(quest.QuestId);

                if (hasQuestId && quest.AutoRegisterMission && GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterMission(new Mission(quest.QuestId, quest.Title, quest.Description));
                }

                if (quest.HasSteps && hasQuestId && !_questRuntimes.ContainsKey(quest.QuestId))
                {
                    _questRuntimes.Add(quest.QuestId, new AlienQuestRuntime(quest));
                }
            }
        }

        private void OnDestroy()
        {
            if (AlienManager.Instance != null)
            {
                AlienManager.Instance.UnregisterAlien(this);
            }
        }

        private void ApplyAnimFromEmotion()
        {
            if (_anim != null)
            {
                _anim.SetInteger(EmotionHash, (int)Emotion);
            }
        }

        public void OnPlayerCombo(Emotion playerEmotion, Behavior channel)
        {
            if (_def == null || _def.Reactions == null)
            {
                return;
            }

            if (!_def.Reactions.TryFindRule(channel, playerEmotion, IsInteractionRuleAvailable, out var rule))
            {
                return;
            }

            HandleInteractionRule(rule, channel);
        }

        public bool TryReceiveItem(string itemId)
        {
            if (_def == null || _def.Reactions == null)
            {
                return false;
            }

            if (!_def.Reactions.TryFindItemRule(itemId, out var rule))
            {
                return false;
            }

            var quantity = GetUpdatedItemQuantity(itemId);
            if (quantity < rule.ExpectedItemQuantity)
            {
                return true;
            }

            ResetItemQuantity(itemId);
            HandleItemRule(rule, itemId);
            return true;
        }

        public bool IsWithinReceiveRadius(Vector3 position)
        {
            var diff = transform.position - position;
            return diff.sqrMagnitude <= _receiveRadius * _receiveRadius;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _receiveRadius);
        }

        internal void SetEmotion(Emotion newEmotion)
        {
            if (Emotion == newEmotion)
            {
                return;
            }

            Emotion = newEmotion;
            ApplyAnimFromEmotion();
        }

        internal void ShowDialogue(string emojiLine, float duration)
        {
            if (_dialogueBubble == null || string.IsNullOrWhiteSpace(emojiLine) || duration <= 0f)
            {
                return;
            }

            _dialogueBubble.ShowFor(emojiLine, duration);
        }

        private void HandleInteractionRule(InterractionRule rule, Behavior channel)
        {
            var handled = ProcessQuestStep(rule.QuestId, rule.QuestStepId, QuestStepType.Talk);

            if (rule.SetNewEmotion)
            {
                SetEmotion(rule.NewEmotion);
            }

            TryShowDialogue(Emotion, channel);
            ApplySuspicionDelta(rule.SuspicionDelta);

            if (!handled && !string.IsNullOrWhiteSpace(rule.QuestId))
            {
                GameManager.Instance?.SetMissionFinished(rule.QuestId);
            }
        }

        private bool IsInteractionRuleAvailable(InterractionRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.QuestId) || string.IsNullOrWhiteSpace(rule.QuestStepId))
            {
                return true;
            }

            return IsQuestStepActive(rule.QuestId, rule.QuestStepId, QuestStepType.Talk);
        }

        private void HandleItemRule(ItemRule rule, string itemId)
        {
            var handled = ProcessQuestStep(rule.QuestId, rule.QuestStepId, QuestStepType.GiveItem);

            if (rule.SetIfGoodItem)
            {
                SetEmotion(rule.NewEmotionIfGoodItem);
            }

            TryShowItemDialogue(itemId);
            ApplySuspicionDelta(rule.SuspicionDelta);

            if (!handled && !string.IsNullOrWhiteSpace(rule.QuestId))
            {
                GameManager.Instance?.SetMissionFinished(rule.QuestId);
            }
        }

        private void TryShowDialogue(Emotion emotion, Behavior behavior)
        {
            if (_dialogueBubble == null || _def?.Dialogue == null)
            {
                return;
            }

            if (_def.Dialogue.TryGet(emotion, behavior, out var entry))
            {
                ShowDialogue(entry.EmojiLine, entry.Duration);
            }
        }

        private void TryShowItemDialogue(string itemId)
        {
            if (_dialogueBubble == null || _def?.Dialogue == null)
            {
                return;
            }

            if (_def.Dialogue.TryGet(itemId, out var entry))
            {
                ShowDialogue(entry.EmojiLine, entry.Duration);
            }
        }

        private void ApplySuspicionDelta(int delta)
        {
            if (delta == 0 || MistrustManager.Instance == null)
            {
                return;
            }

            if (delta > 0)
            {
                MistrustManager.Instance.AddMistrust(delta);
            }
            else
            {
                MistrustManager.Instance.RemoveMistrust(-delta);
            }
        }

        private bool ProcessQuestStep(string questId, string questStepId, QuestStepType triggerType)
        {
            if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(questStepId))
            {
                return false;
            }

            if (_questRuntimes.TryGetValue(questId, out var runtime))
            {
                return runtime.TryHandleStep(questStepId, triggerType);
            }

            return false;
        }

        private bool IsQuestStepActive(string questId, string questStepId, QuestStepType triggerType)
        {
            if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(questStepId))
            {
                return false;
            }

            if (_questRuntimes.TryGetValue(questId, out var runtime))
            {
                return runtime.IsStepActive(questStepId, triggerType);
            }

            return false;
        }

        private int GetUpdatedItemQuantity(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0;
            }

            if (_receivedItemQuantities.TryGetValue(itemId, out var quantity))
            {
                quantity++;
                _receivedItemQuantities[itemId] = quantity;
                return quantity;
            }

            _receivedItemQuantities[itemId] = 1;
            return 1;
        }

        private void ResetItemQuantity(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            _receivedItemQuantities[itemId] = 0;
        }
    }
}
