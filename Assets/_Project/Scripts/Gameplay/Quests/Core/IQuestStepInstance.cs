namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Runtime representation of a quest step. Implementations encapsulate
    /// step-specific logic and state.
    /// </summary>
    public interface IQuestStepInstance
    {
        string StepId { get; }
        QuestSignalType ExpectedSignal { get; }
        bool CompletesQuest { get; }
        string NextStepId { get; }

        void OnEnter(QuestRuntimeContext context);
        void OnExit(QuestRuntimeContext context);
        bool CanProgress(QuestRuntimeContext context, in QuestSignal signal);
    }
}
