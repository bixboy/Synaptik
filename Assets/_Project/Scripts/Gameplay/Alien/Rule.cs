
    using System;
    using NaughtyAttributes;
    using UnityEngine;

    [Serializable]
    public struct Rule
    {
        [SerializeField] private Behavior _channel;
        [SerializeField] private Emotion _playerEmotion;
        [SerializeField] private AlienVerb _verb;
 
        [SerializeField] private int _suspicionDelta;
        [SerializeField] private bool _setNewEmotion;
        [SerializeField, ShowIf("_setNewEmotion")] private AlienEmotion _newEmotion;
            
            
        [Header("Item Reaction")]
        [SerializeField] private bool _dependsOnItem;
        [SerializeField] private string _expectedItemId;
        [SerializeField] private int _expectedItemQuantity;
        [SerializeField] private bool _setIfGoodItem;
        [SerializeField, ShowIf("_setIfGoodItem")] private AlienEmotion _newEmotionIfGoodItem;

        public Behavior Channel => _channel;
        public Emotion PlayerEmotion => _playerEmotion;
        public AlienVerb Verb => _verb;
        public int SuspicionDelta => _suspicionDelta;
        public bool SetNewEmotion => _setNewEmotion;
        public AlienEmotion NewEmotion => _newEmotion;
        public bool DependsOnItem => _dependsOnItem;
        public string ExpectedItemId => _expectedItemId;
        
        public int ExpectedItemQuantity => _expectedItemQuantity;
        public bool SetIfGoodItem => _setIfGoodItem;
        public AlienEmotion NewEmotionIfGoodItem => _newEmotionIfGoodItem;
    }
