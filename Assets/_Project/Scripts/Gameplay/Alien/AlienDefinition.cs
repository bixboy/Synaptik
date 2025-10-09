using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Synaptik.Game
{
    [CreateAssetMenu(menuName = "Synaptik/Alien/Definition", fileName = "AlienDefinition")]
    public class AlienDefinition : ScriptableObject
    {
        [SerializeField, ReadOnly] private string _alienId;
        [SerializeField] private Emotion _startEmotion = Emotion.Curious;
        [SerializeField] private RuntimeAnimatorController _animator;
        [SerializeField] private ReactionMatrix _reactions;
        [SerializeField] private DialogueDatabase _dialogue;
        [SerializeField] private List<AlienQuest> _quests = new List<AlienQuest>();

        public string AlienId => _alienId;
        public Emotion StartEmotion => _startEmotion;

        public RuntimeAnimatorController Animator => _animator;
        public ReactionMatrix Reactions => _reactions;
        public DialogueDatabase Dialogue => _dialogue;
        
        public List<AlienQuest> Quests => _quests;
        
        public void SetUniqueId(string id)
        {
            _alienId = id;
        }
    }
}