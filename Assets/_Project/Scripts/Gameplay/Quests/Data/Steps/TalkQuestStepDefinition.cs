using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    [CreateAssetMenu(menuName = "Synaptik/Quests/Steps/Talk", fileName = "TalkQuestStep")]
    public sealed class TalkQuestStepDefinition : QuestStepDefinition
    {
        [SerializeField]
        private bool requireSpecificSpeaker;

        [SerializeField, Tooltip("Optional speaker identifier (e.g. player id, NPC id).")]
        private string speakerId;

        protected override bool OnCanProgress(QuestRuntimeContext context, in QuestSignal signal)
        {
            if (!requireSpecificSpeaker)
            {
                return true;
            }

            if (signal.TryGetPayload<TalkSignalPayload>(out var payload))
            {
                return payload != null && payload.SpeakerId == speakerId;
            }

            return false;
        }
    }

    public sealed class TalkSignalPayload : IQuestSignalPayload
    {
        public string SpeakerId { get; }

        public TalkSignalPayload(string speakerId)
        {
            SpeakerId = speakerId;
        }
    }
}
