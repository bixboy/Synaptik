using System;
using UnityEngine;

namespace Synaptik.Game
{
    [RequireComponent(typeof(Animator))]
    public class Alien : MonoBehaviour
    {
        [SerializeField] private AlienDefinition _def;
        public AlienDefinition Definition => _def;
        [SerializeField] private float _receiveRadius = 1.3f;
        [SerializeField] private DialogueBubble _dialogueBubblePrefab;

        public Emotion Emotion { get; private set; }

        private Animator _anim;
        private static readonly int EmotionHash = Animator.StringToHash("Emotion");
        
        
        private int itemQuantity = 0;
        public int ItemQuantity => itemQuantity;

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            if (_def != null && _def.Animator != null)
            {
                _anim.runtimeAnimatorController = _def.Animator;
            }

            Emotion = _def != null ? _def.StartEmotion : Emotion.Curious;
            Debug.Log("Emotion at start: " + Emotion);
            ApplyAnimFromEmotion();
        }

        private void Start()
        {
            AlienManager.Instance.RegisterAlien(this);
            foreach (AlienQuest quest in _def.Quests)
            {
                GameManager.Instance.RegisterMission(new Mission(quest.QuestId, quest.Title, quest.Description));
            }
        }

        private void OnDestroy()
        {
            AlienManager.Instance.UnregisterAlien(this);
        }

        private void ApplyAnimFromEmotion()
        {
            return;
            if (_anim != null)
            {
                _anim.SetInteger(EmotionHash, (int)Emotion);
            }
        }

        public void OnPlayerCombo(Emotion playerEmotion, Behavior channel)
        {
            Debug.Log("Player combo: " + playerEmotion);
            if (_def == null || _def.Reactions == null)
            {
                Debug.Log("No definition or reactions");
                return;
            }
            
            if (_def.Reactions.TryFindRule(channel, playerEmotion, out var rule))
            {
                if (rule.SetNewEmotion)
                {
                    Emotion = rule.NewEmotion;
                    ApplyAnimFromEmotion();
                    Debug.Log(rule.NewEmotion);
                }

                if (_def.Dialogue != null && _def.Dialogue.TryGet(Emotion, channel, out var entry))
                {
                    _dialogueBubblePrefab.ShowFor(entry.EmojiLine, entry.Duration);
                }
                
                MistrustManager.Instance.AddMistrust(rule.SuspicionDelta);
                if (rule.QuestId != null)
                {
                    GameManager.Instance.SetMissionFinished(rule.QuestId);
                }
                
            }
            
        }

        public bool TryReceiveItem(string itemId)
        {
            if (_def == null || _def.Reactions == null)
            {
                Debug.Log("No definition or reactions");
                return false;
            }

            if (!_def.Reactions.TryFindItemRule(itemId, out var rule))
            {
                Debug.Log($"No rule for item {itemId}");
                return false;
            }
            
            Debug.Log($"Rule found for item {itemId}: expected {rule.ExpectedItemId}, quantity {rule.ExpectedItemQuantity}, set if good {rule.SetIfGoodItem}, new emotion if good {rule.NewEmotionIfGoodItem}");
            itemQuantity++;
            if (itemQuantity >= rule.ExpectedItemQuantity)
            {
                itemQuantity = 0;
                if (rule.SetIfGoodItem)
                {
                    Emotion = rule.NewEmotionIfGoodItem;
                    ApplyAnimFromEmotion();
                    Debug.Log(rule.NewEmotionIfGoodItem);
                }
                
                if (_def.Dialogue != null && _def.Dialogue.TryGet(itemId, out var entry)) // On reçoit un item, donc on utilise le channel Action forcémment (le joueur ne peut pas donner un objet en parlant)
                {
                    _dialogueBubblePrefab.ShowFor(entry.EmojiLine, entry.Duration);
                    Debug.Log($"Dialogue for item {itemId}: {entry.EmojiLine}" );
                }
            
                MistrustManager.Instance.AddMistrust(rule.SuspicionDelta);
                if (rule.QuestId != null)
                {
                    GameManager.Instance.SetMissionFinished(rule.QuestId);
                }
            }
            
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
    }
}
