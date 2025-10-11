using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class Alien : MonoBehaviour, IInteraction
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
    private readonly Dictionary<InteractionLookupKey, InterractionRule> _cachedInteractionRules = new Dictionary<InteractionLookupKey, InterractionRule>();

    private void Awake()
    {
        _anim = GetComponent<Animator>();

        if (!_dialogueBubble)
            _dialogueBubble = GetComponentInChildren<DialogueBubble>(true);
        
        if (_dialogueBubble && !_dialogueBubble.gameObject.scene.IsValid())
        {
            var prefabBubble = _dialogueBubble;
            _dialogueBubble = Instantiate(prefabBubble, transform);
            _dialogueBubble.transform.localPosition = prefabBubble.transform.localPosition;
            _dialogueBubble.transform.localRotation = prefabBubble.transform.localRotation;
            _dialogueBubble.transform.localScale = prefabBubble.transform.localScale;
        }

        if (_def && _def.Animator)
        {
            _anim.runtimeAnimatorController = _def.Animator;
        }

        Emotion = _def ? _def.StartEmotion : Emotion.Curious;
        ApplyAnimFromEmotion();
    }

    private void Start()
    {
        if (AlienManager.Instance)
            AlienManager.Instance.RegisterAlien(this);

        if (!_def)
            return;

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
        if (AlienManager.Instance)
            AlienManager.Instance.UnregisterAlien(this);
    }

    private void ApplyAnimFromEmotion()
    {
        if (_anim)
        {
            _anim.SetInteger(EmotionHash, (int)Emotion);
        }
    }
    
    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        Behavior behavior = action._behavior;
        Emotion emotion = action._emotion;
        
        if (behavior == Behavior.Action)
        {
            switch (emotion)
            {
                case Emotion.Anger: // Hit
                    break;
                
                case Emotion.Curious: // Ramasser
                    break;
                
                case Emotion.Fearful: // Courir
                    break;
                
                case Emotion.Friendly: // Donne
                {
                    if (item && TryReceiveItem(item.ItemId))
                    {
                        playerInteraction.DropItem(true);
                        Debug.Log($"Give item {item.ItemId} to alien {Definition.name}");
                        return;
                    }
                    
                    if (item)
                    {
                        playerInteraction?.DropItem();
                        Debug.Log("Drop item in front of alien");
                        return;
                    }
                    break;
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
        
        OnPlayerCombo(action._emotion, action._behavior);
    }

    public void OnPlayerCombo(Emotion playerEmotion, Behavior channel)
    {
        if (!_def || !_def.Reactions)
            return;

        Debug.Log($"[Alien] {name} received combo {channel}/{playerEmotion}");

        var interactionKey = new InteractionLookupKey(channel, playerEmotion);
        if (!_def.Reactions.TryFindRule(channel, playerEmotion, IsInteractionRuleAvailable, out var rule))
        {
            if (_cachedInteractionRules.TryGetValue(interactionKey, out var cachedRule))
            {
                Debug.Log($"[Alien] No interaction rule available for {name} with combo {channel}/{playerEmotion}. Using cached fallback.");
                
                HandleInteractionRule(cachedRule, channel, playerEmotion, false);
                return;
            }

            if (_def.Reactions.TryFindRule(channel, playerEmotion, null, out var fallbackRule))
            {
                Debug.Log($"[Alien] No interaction rule available for {name} with combo {channel}/{playerEmotion}. Using definition fallback.");
                
                _cachedInteractionRules[interactionKey] = fallbackRule;
                HandleInteractionRule(fallbackRule, channel, playerEmotion, false);
                
                return;
            }

            Debug.LogWarning($"[Alien] No interaction rule found for {name} with combo {channel}/{playerEmotion}");
            return;
        }

        _cachedInteractionRules[interactionKey] = rule;
        HandleInteractionRule(rule, channel, playerEmotion);
    }

    public bool TryReceiveItem(string itemId)
    {
        if (!_def || !_def.Reactions)
            return false;

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
            return;

        Emotion = newEmotion;
        ApplyAnimFromEmotion();
    }

    internal void ShowDialogue(string emojiLine, float duration)
    {
        if (!_dialogueBubble || string.IsNullOrWhiteSpace(emojiLine) || duration <= 0f)
            return;

        _dialogueBubble.ShowFor(emojiLine, duration);
    }

    private void HandleInteractionRule(InterractionRule rule, Behavior channel, Emotion playerEmotion, bool allowQuestProgress = true)
    {
        var handled = allowQuestProgress && ProcessQuestStep(rule.QuestId, rule.QuestStepId, QuestStepType.Talk);

        if (rule.SetNewEmotion)
        {
            SetEmotion(rule.NewEmotion);
        }

        if (!TryShowDialogue(playerEmotion, channel, "player emotion"))
        {
            var displayEmotion = Emotion;
            if (rule.SetNewEmotion)
            {
                displayEmotion = rule.NewEmotion;
            }

            if (displayEmotion != playerEmotion)
            {
                TryShowDialogue(displayEmotion, channel, "alien emotion fallback");
            }
        }
        ApplySuspicionDelta(rule.SuspicionDelta);

        if (allowQuestProgress && !handled && !string.IsNullOrWhiteSpace(rule.QuestId))
        {
            GameManager.Instance?.SetMissionFinished(rule.QuestId);
        }
    }

    private bool IsInteractionRuleAvailable(InterractionRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.QuestId))
            return true;

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

    private bool TryShowDialogue(Emotion emotion, Behavior behavior, string source)
    {
        if (!_dialogueBubble || !_def?.Dialogue)
        {
            Debug.LogWarning($"[Alien] Cannot show dialogue for {name}. Missing dialogue bubble or database.");
            return false;
        }

        if (_def.Dialogue.TryGet(emotion, behavior, out var entry))
        {
            Debug.Log($"[Alien] Showing dialogue '{entry.EmojiLine}' from {source} for {name} using key {behavior}/{emotion}.");
            
            ShowDialogue(entry.EmojiLine, entry.Duration);
            return true;
        }

        Debug.LogWarning($"[Alien] No dialogue entry found for {name} using key {behavior}/{emotion} from {source}.");
        
        return false;
    }

    private void TryShowItemDialogue(string itemId)
    {
        if (!_dialogueBubble || !_def?.Dialogue)
            return;

        if (_def.Dialogue.TryGet(itemId, out var entry))
        {
            ShowDialogue(entry.EmojiLine, entry.Duration);
        }
    }

    private void ApplySuspicionDelta(int delta)
    {
        if (delta == 0 || !MistrustManager.Instance)
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
        if (string.IsNullOrWhiteSpace(questId))
        {
            Debug.Log($"[Alien] {name} processed interaction without quest (trigger {triggerType}).");
            return false;
        }

        if (_questRuntimes.TryGetValue(questId, out var runtime))
        {
            var handled = runtime.TryHandleStep(questStepId, triggerType);
            Debug.Log($"[Alien] {name} quest '{questId}' step '{questStepId ?? "<current>"}' handled={handled} for trigger {triggerType}.");
            
            return handled;
        }

        Debug.LogWarning($"[Alien] {name} has no runtime for quest '{questId}'.");
        return false;
    }

    private bool IsQuestStepActive(string questId, string questStepId, QuestStepType triggerType)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            Debug.Log($"[Alien] {name} checking interaction rule without quest binding (trigger {triggerType}).");
            return false;
        }

        if (_questRuntimes.TryGetValue(questId, out var runtime))
        {
            var active = runtime.IsStepActive(questStepId, triggerType);
            Debug.Log($"[Alien] {name} quest '{questId}' step '{questStepId ?? "<current>"}' active={active} for trigger {triggerType}.");
            
            return active;
        }

        Debug.LogWarning($"[Alien] {name} has no runtime for quest '{questId}' while checking rule availability.");
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

    private readonly struct InteractionLookupKey : IEquatable<InteractionLookupKey>
    {
        private readonly Behavior _behavior;
        private readonly Emotion _emotion;

        public InteractionLookupKey(Behavior behavior, Emotion emotion)
        {
            _behavior = behavior;
            _emotion = emotion;
        }

        public bool Equals(InteractionLookupKey other)
        {
            return _behavior == other._behavior && _emotion == other._emotion;
        }

        public override bool Equals(object obj)
        {
            return obj is InteractionLookupKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)_behavior * 397) ^ (int)_emotion;
            }
        }
    }



}
