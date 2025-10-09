using System;
using UnityEngine;

namespace Synaptik.Game
{
    [CreateAssetMenu(menuName = "Synaptik/Alien/Dialogue Database", fileName = "DialogueDatabase")]
    public class DialogueDatabase : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField] private Emotion _emotion;
            [SerializeField] private Behavior _behavior;
            [SerializeField] private string _emojiLine;
            [SerializeField] private float _duration;

            public Emotion Emotion => _emotion;
            public Behavior Behavior => _behavior;
            public string EmojiLine => _emojiLine;
            public float Duration => _duration <= 0f ? 2f : _duration;
        }
        
        [Serializable]
        public struct ItemEntry
        {
            [SerializeField] private string _itemID;
            [SerializeField] private string _emojiLine;
            [SerializeField] private float _duration;

            public string ItemId => _itemID;
            public string EmojiLine => _emojiLine;
            public float Duration => _duration <= 0f ? 2f : _duration;
        }


        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        [SerializeField] private ItemEntry[] _itemEntries = Array.Empty<ItemEntry>();

        public bool TryGet(Emotion emotion, Behavior behavior, out Entry entry)
        {
            var entries = _entries;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Emotion == emotion && entries[i].Behavior == behavior)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
        
        public bool TryGet(string itemID, out ItemEntry entry)
        {
            var itemEntries = _itemEntries;
            for (int i = 0; i < _itemEntries.Length; i++)
            {
                if (itemEntries[i].ItemId == itemID)
                {
                    entry = _itemEntries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
        
        
    }
}