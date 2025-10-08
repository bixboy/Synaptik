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
            [SerializeField] private AlienEmotion _emotion;
            [SerializeField] private AlienVerb _verb;
            [SerializeField] private string _emojiLine;
            [SerializeField] private float _duration;

            public AlienEmotion Emotion => _emotion;
            public AlienVerb Verb => _verb;
            public string EmojiLine => _emojiLine;
            public float Duration => _duration <= 0f ? 2f : _duration;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public bool TryGet(AlienEmotion emotion, AlienVerb verb, out Entry entry)
        {
            var entries = _entries;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Emotion == emotion && entries[i].Verb == verb)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }
    }
}