using System.Collections.Generic;
using Synaptik.Interfaces;
using Unity.Collections;
using UnityEngine;

namespace Synaptik.Gameplay.Alien
{
    [CreateAssetMenu(menuName = "Synaptik/Alien/Definition", fileName = "AlienDefinition")]
    public class AlienDefinition : ScriptableObject
    {
        [SerializeField, ReadOnly]
        private string alienId;

        [SerializeField]
        private Emotion startEmotion = Emotion.Curious;

        [SerializeField]
        private RuntimeAnimatorController animator;

        [SerializeField]
        private ReactionMatrix reactions;

        [SerializeField]
        private DialogueDatabase dialogue;

        [SerializeField]
        private List<AlienQuest> quests = new();

        public string AlienId => alienId;
        public Emotion StartEmotion => startEmotion;
        public RuntimeAnimatorController Animator => animator;
        public ReactionMatrix Reactions => reactions;
        public DialogueDatabase Dialogue => dialogue;
        public List<AlienQuest> Quests => quests;

        public void SetUniqueId(string id)
        {
            alienId = id;
        }
    }
}
