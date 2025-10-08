
    using System;
    using NaughtyAttributes;
    using UnityEngine;

    [Serializable]
    public struct Rule
    {
        [SerializeField] private Behavior _channel;
        [SerializeField] private Emotion _playerEmotion;
 
        [SerializeField] private int _suspicionDelta;
        [SerializeField] private bool _setNewEmotion;
        [SerializeField] private Emotion _newEmotion;
            
            
        [Header("Item Reaction")]
        [SerializeField] private bool _dependsOnItem;
        [SerializeField] private string _expectedItemId;
        [SerializeField] private int _expectedItemQuantity;
        [SerializeField] private bool _setIfGoodItem;
        [SerializeField] private Emotion _newEmotionIfGoodItem;

        public Behavior Channel => _channel;
        public Emotion PlayerEmotion => _playerEmotion;

        public int SuspicionDelta => _suspicionDelta;
        public bool SetNewEmotion => _setNewEmotion;
        public Emotion NewEmotion => _newEmotion;
        public bool DependsOnItem => _dependsOnItem;
        public string ExpectedItemId => _expectedItemId;
        
        public int ExpectedItemQuantity => _expectedItemQuantity;
        public bool SetIfGoodItem => _setIfGoodItem;
        public Emotion NewEmotionIfGoodItem => _newEmotionIfGoodItem;
    }
