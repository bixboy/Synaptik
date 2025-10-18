using System;
using System.Collections.Generic;

namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Stateful runtime for a quest definition. Handles step transitions and
    /// raises events for UI/gameplay code.
    /// </summary>
    public sealed class QuestRuntime
    {
        private readonly Dictionary<string, IQuestStepInstance> _stepsById = new();
        private readonly List<IQuestStepInstance> _steps = new();
        private readonly QuestDefinition _definition;
        private readonly QuestRuntimeContext _context;

        private IQuestStepInstance _currentStep;

        public QuestDefinition Definition => _definition;
        public bool IsCompleted { get; private set; }

        public event Action<QuestRuntime, IQuestStepInstance> OnStepStarted;
        public event Action<QuestRuntime, IQuestStepInstance> OnStepCompleted;
        public event Action<QuestRuntime> OnQuestCompleted;

        public QuestRuntime(QuestDefinition definition, QuestRuntimeContext context)
        {
            _definition = definition;
            _context = context;

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.Steps == null)
            {
                return;
            }

            for (int i = 0; i < definition.Steps.Count; i++)
            {
                var stepDefinition = definition.Steps[i];
                if (stepDefinition == null)
                {
                    continue;
                }

                var instance = new QuestStepInstance(stepDefinition);
                _steps.Add(instance);
                if (!_stepsById.ContainsKey(instance.StepId))
                {
                    _stepsById.Add(instance.StepId, instance);
                }
            }
        }

        public void Start()
        {
            if (_definition == null || IsCompleted)
            {
                return;
            }

            if (_definition.GetFirstStep() == null)
            {
                return;
            }

            if (_definition.TryGetStep(_definition.GetFirstStep().StepId, out var definition))
            {
                ActivateStep(definition.StepId);
            }
        }

        public bool IsStepActive(string stepId, QuestSignalType signalType)
        {
            if (IsCompleted || _currentStep == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(stepId) && _currentStep.StepId != stepId)
            {
                return false;
            }

            return _currentStep.ExpectedSignal == signalType;
        }

        public bool TryProcessSignal(in QuestSignal signal)
        {
            if (IsCompleted || _currentStep == null)
            {
                return false;
            }

            if (!string.Equals(signal.QuestId, _definition.QuestId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(signal.StepId) && !string.Equals(signal.StepId, _currentStep.StepId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!_currentStep.CanProgress(_context, signal))
            {
                return false;
            }

            CompleteCurrentStep();
            return true;
        }

        private void CompleteCurrentStep()
        {
            var completedStep = _currentStep;
            completedStep.OnExit(_context);
            OnStepCompleted?.Invoke(this, completedStep);

            if (completedStep.CompletesQuest)
            {
                IsCompleted = true;
                OnQuestCompleted?.Invoke(this);
                _currentStep = null;
                return;
            }

            var nextStepId = ResolveNextStepId(completedStep);
            if (!string.IsNullOrWhiteSpace(nextStepId))
            {
                ActivateStep(nextStepId);
                return;
            }

            // Default to sequential progression when no explicit id is provided.
            var index = _steps.IndexOf(completedStep);
            if (index >= 0 && index + 1 < _steps.Count)
            {
                ActivateStep(_steps[index + 1].StepId);
            }
            else
            {
                IsCompleted = true;
                OnQuestCompleted?.Invoke(this);
                _currentStep = null;
            }
        }

        private void ActivateStep(string stepId)
        {
            if (!_stepsById.TryGetValue(stepId, out var nextStep))
            {
                return;
            }

            _currentStep = nextStep;
            _currentStep.OnEnter(_context);
            OnStepStarted?.Invoke(this, _currentStep);
        }

        private static string ResolveNextStepId(IQuestStepInstance step)
        {
            return string.IsNullOrWhiteSpace(step.NextStepId) ? null : step.NextStepId;
        }
    }
}
