using UnityEngine;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Base class for all quest step authoring assets. Steps should be created as
    /// sub-assets of a <see cref="QuestDefinition"/> so they can be reused across
    /// aliens or quests.
    /// </summary>
    public abstract class QuestStepDefinition : ScriptableObject
    {
        [SerializeField]
        private string stepId;

        [SerializeField]
        private QuestSignalType expectedSignal = QuestSignalType.Talk;

        [SerializeField]
        private bool completesQuest;

        [SerializeField]
        private string nextStepId;

        public string StepId => string.IsNullOrWhiteSpace(stepId) ? name : stepId;
        public QuestSignalType ExpectedSignal => expectedSignal;
        public bool CompletesQuest => completesQuest;
        public string NextStepId => nextStepId;

        internal bool InternalEvaluate(QuestRuntimeContext context, in QuestSignal signal)
        {
            return OnCanProgress(context, signal);
        }

        internal void InternalEnter(QuestRuntimeContext context)
        {
            OnEnter(context);
        }

        internal void InternalExit(QuestRuntimeContext context)
        {
            OnExit(context);
        }

        /// <summary>
        /// Override to check whether the provided signal is valid for this step.
        /// </summary>
        protected abstract bool OnCanProgress(QuestRuntimeContext context, in QuestSignal signal);

        /// <summary>
        /// Optional hook executed when the step becomes active.
        /// </summary>
        protected virtual void OnEnter(QuestRuntimeContext context) { }

        /// <summary>
        /// Optional hook executed when the step is completed.
        /// </summary>
        protected virtual void OnExit(QuestRuntimeContext context) { }
    }
}
