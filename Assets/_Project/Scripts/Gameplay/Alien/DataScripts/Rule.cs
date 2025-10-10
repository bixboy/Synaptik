using System;
using NaughtyAttributes;
using UnityEngine;

namespace Synaptik.Game
{
    [Serializable]
    public struct InterractionRule
    {
        [SerializeField] private Behavior _channel;
        [SerializeField] private Emotion _playerEmotion;
        [SerializeField] private string _questId;
        [SerializeField] private string _questStepId;
        [SerializeField] private int _suspicionDelta;
        [SerializeField] private bool _setNewEmotion;
        [SerializeField] private Emotion _newEmotion;

        public Behavior Channel => _channel;
        public Emotion PlayerEmotion => _playerEmotion;
        public string QuestId => _questId;
        public string QuestStepId => _questStepId;
        public int SuspicionDelta => _suspicionDelta;
        public bool SetNewEmotion => _setNewEmotion;
        public Emotion NewEmotion => _newEmotion;
    }

    [Serializable]
    public struct ItemRule
    {
        [Header("Item Reaction")]
        [SerializeField] private string _questId;
        [SerializeField] private string _questStepId;
        [SerializeField] private string _expectedItemId;
        [SerializeField] private int _suspicionDelta;
        [SerializeField] private int _expectedItemQuantity;
        [SerializeField] private bool _setIfGoodItem;
        [SerializeField] private Emotion _newEmotionIfGoodItem;

        public string QuestId => _questId;
        public string QuestStepId => _questStepId;
        public string ExpectedItemId => _expectedItemId;
        public int SuspicionDelta => _suspicionDelta;
        public int ExpectedItemQuantity => _expectedItemQuantity <= 0 ? 1 : _expectedItemQuantity;
        public bool SetIfGoodItem => _setIfGoodItem;
        public Emotion NewEmotionIfGoodItem => _newEmotionIfGoodItem;
    }
}
