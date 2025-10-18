namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Default implementation bridging quest step definitions and runtime state.
    /// </summary>
    public sealed class QuestStepInstance : IQuestStepInstance
    {
        private readonly QuestStepDefinition _definition;

        public QuestStepInstance(QuestStepDefinition definition)
        {
            _definition = definition;
        }

        public string StepId => _definition.StepId;
        public QuestSignalType ExpectedSignal => _definition.ExpectedSignal;
        public bool CompletesQuest => _definition.CompletesQuest;
        public string NextStepId => _definition.NextStepId;

        public void OnEnter(QuestRuntimeContext context)
        {
            _definition.InternalEnter(context);
        }

        public void OnExit(QuestRuntimeContext context)
        {
            _definition.InternalExit(context);
        }

        public bool CanProgress(QuestRuntimeContext context, in QuestSignal signal)
        {
            if (signal.SignalType != ExpectedSignal)
            {
                return false;
            }

            return _definition.InternalEvaluate(context, signal);
        }
    }
}
