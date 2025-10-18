using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Enumerates the known quest signal channels.
    /// Designers can freely extend this list by adding new values and
    /// implementing step definitions that react to them.
    /// </summary>
    public enum QuestSignalType
    {
        Talk = 0,
        GiveItem = 1,
    }

    /// <summary>
    /// Marker interface for strongly typed payloads attached to quest signals.
    /// </summary>
    public interface IQuestSignalPayload
    {
    }

    /// <summary>
    /// Immutable container passed through the quest system when something happens in game.
    /// </summary>
    public readonly struct QuestSignal
    {
        public string QuestId { get; }
        public string StepId { get; }
        public QuestSignalType SignalType { get; }
        public IQuestSignalPayload Payload { get; }
        public Object Sender { get; }

        public QuestSignal(string questId, string stepId, QuestSignalType signalType, IQuestSignalPayload payload, Object sender)
        {
            QuestId = questId;
            StepId = stepId;
            SignalType = signalType;
            Payload = payload;
            Sender = sender;
        }

        public bool TryGetPayload<TPayload>(out TPayload payload) where TPayload : class, IQuestSignalPayload
        {
            payload = Payload as TPayload;
            return payload != null;
        }
    }
}
