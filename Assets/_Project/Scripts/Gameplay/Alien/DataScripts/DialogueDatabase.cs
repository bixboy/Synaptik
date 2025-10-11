using System;
using Synaptik.Interfaces;
using UnityEngine;

namespace Synaptik.Gameplay.Alien
{
    [CreateAssetMenu(menuName = "Synaptik/Alien/Dialogue Database", fileName = "DialogueDatabase")]
    public class DialogueDatabase : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField]
            private Emotion emotion;

            [SerializeField]
            private Behavior behavior;

            [SerializeField]
            private string emojiLine;

            [SerializeField]
            private float duration;

            public Emotion Emotion => emotion;
            public Behavior Behavior => behavior;
            public string EmojiLine => emojiLine;
            public float Duration => duration <= 0f ? 2f : duration;
        }

        [Serializable]
        public struct ItemEntry
        {
            [SerializeField]
            private string itemId;

            [SerializeField]
            private string emojiLine;

            [SerializeField]
            private float duration;

            public string ItemId => itemId;
            public string EmojiLine => emojiLine;
            public float Duration => duration <= 0f ? 2f : duration;
        }

        [SerializeField]
        private Entry[] entries = Array.Empty<Entry>();

        [SerializeField]
        private ItemEntry[] itemEntries = Array.Empty<ItemEntry>();

        public bool TryGet(Emotion emotion, Behavior behavior, out Entry entry)
        {
            for (var i = 0; i < entries.Length; i++)
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

        public bool TryGet(string itemId, out ItemEntry entry)
        {
            for (var i = 0; i < itemEntries.Length; i++)
            {
                if (itemEntries[i].ItemId == itemId)
                {
                    entry = itemEntries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}
