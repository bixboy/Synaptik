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
        public AlienState State { get; private set; }
        public AlienEmotion Emotion { get; private set; }

        private Animator _anim;
        private static readonly int EmotionHash = Animator.StringToHash("Emotion");
        
        
        private int itemQuantity = 0;

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            if (_def != null && _def.Animator != null)
            {
                _anim.runtimeAnimatorController = _def.Animator;
            }

            Emotion = _def != null ? _def.StartEmotion : AlienEmotion.Curious;
            State = AlienState.Idle;

            ApplyAnimFromEmotion();
        }

        private void Start()
        {
            AlienManager.Instance.RegisterAlien(this);
        }

        private void OnDestroy()
        {
            AlienManager.Instance.UnregisterAlien(this);
        }

        private void ApplyAnimFromEmotion()
        {
            if (_anim != null)
            {
                _anim.SetInteger(EmotionHash, (int)Emotion);
            }
        }

        public void OnPlayerCombo(Emotion playerEmotion, Behavior channel, AlienVerb verb, GameObject playerGO)
        {
            if (_def == null || _def.Reactions == null)
            {
                return;
            }

            AlienVerb resolvedVerb = verb;

            if (_def.Reactions.TryFindRule(channel, playerEmotion, out var rule))
            {
                if (rule.SetNewEmotion)
                {
                    Emotion = rule.NewEmotion;
                    ApplyAnimFromEmotion();
                }

                if (_def.Dialogue != null && _def.Dialogue.TryGet(Emotion, resolvedVerb, out var entry))
                {
                    DialogueBubble.ShowFor(this, entry.EmojiLine, entry.Duration);
                }
                
            }
            
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
            
            itemQuantity++;
            if (itemQuantity >= rule.ExpectedItemQuantity)
            {
                itemQuantity = 0;
                if (rule.SetIfGoodItem)
                {
                    Emotion = rule.NewEmotionIfGoodItem;
                    ApplyAnimFromEmotion();
                }
            

                if (_def.Dialogue != null && _def.Dialogue.TryGet(Emotion, rule.Verb, out var entry))
                {
                    DialogueBubble.ShowFor(this, entry.EmojiLine, entry.Duration);
                }
            }
            
            return true;
        }

        public bool IsWithinReceiveRadius(Vector3 position)
        {
            var diff = transform.position - position;
            return diff.sqrMagnitude <= _receiveRadius * _receiveRadius;
        }
    }
}
