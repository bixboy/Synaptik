using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Game
{
    public sealed class AlienQuestRuntime
    {
        private readonly AlienQuest _definition;
        private readonly QuestStep[] _steps;
        private readonly Dictionary<string, int> _stepIndexById;
        private int _currentIndex;
        private bool _questCompleted;

        public AlienQuestRuntime(AlienQuest definition)
        {
            _definition = definition;

            var steps = definition.Steps;
            _steps = new QuestStep[steps.Count];
            _stepIndexById = new Dictionary<string, int>(steps.Count);

            for (int i = 0; i < steps.Count; i++)
            {
                _steps[i] = steps[i];
                var runtimeId = steps[i].ResolveStepId(i);

                if (!_stepIndexById.ContainsKey(runtimeId))
                {
                    _stepIndexById.Add(runtimeId, i);
                }
                else
                {
                    Debug.LogWarning($"Duplicate quest step id '{runtimeId}' detected for quest '{definition.QuestId}'. Only the first occurrence will be used.");
                }
            }

            _currentIndex = 0;
        }

        public bool TryHandleStep(string stepId, QuestStepType triggerType)
        {
            if (!_definition.HasSteps || _questCompleted)
            {
                return false;
            }

            if (string.IsNullOrEmpty(stepId))
            {
                return TryHandleCurrentStep(triggerType);
            }

            if (!_stepIndexById.TryGetValue(stepId, out var index))
            {
                return false;
            }

            if (index < 0 || index >= _steps.Length)
            {
                return false;
            }

            if (_steps[index].StepType != triggerType)
            {
                return false;
            }

            if (index != _currentIndex)
            {
                return false;
            }

            ExecuteStep(index);
            return true;
        }

        public bool IsStepActive(string stepId, QuestStepType triggerType)
        {
            if (!_definition.HasSteps || _questCompleted)
            {
                return false;
            }

            if (string.IsNullOrEmpty(stepId))
            {
                return IsCurrentStepActive(triggerType);
            }

            if (!_stepIndexById.TryGetValue(stepId, out var index))
            {
                return false;
            }

            if (index < 0 || index >= _steps.Length)
            {
                return false;
            }

            if (_steps[index].StepType != triggerType)
            {
                return false;
            }

            return index == _currentIndex;
        }

        public bool TryHandleCurrentStep(QuestStepType triggerType)
        {
            if (!_definition.HasSteps || _questCompleted)
            {
                return false;
            }

            if (_currentIndex < 0 || _currentIndex >= _steps.Length)
            {
                return false;
            }

            if (_steps[_currentIndex].StepType != triggerType)
            {
                return false;
            }

            ExecuteStep(_currentIndex);
            return true;
        }

        public bool IsCurrentStepActive(QuestStepType triggerType)
        {
            if (!_definition.HasSteps || _questCompleted)
            {
                return false;
            }

            if (_currentIndex < 0 || _currentIndex >= _steps.Length)
            {
                return false;
            }

            return _steps[_currentIndex].StepType == triggerType;
        }

        private void ExecuteStep(int index)
        {
            var step = _steps[index];

            if (step.CompletesQuest)
            {
                CompleteQuest();
            }
            else
            {
                AdvanceToNext(index, step.NextStepId);
            }
        }

        private void AdvanceToNext(int currentIndex, string nextStepId)
        {
            if (!string.IsNullOrEmpty(nextStepId) && _stepIndexById.TryGetValue(nextStepId, out var explicitIndex))
            {
                _currentIndex = explicitIndex;
                return;
            }

            var sequentialIndex = currentIndex + 1;
            if (sequentialIndex < _steps.Length)
            {
                _currentIndex = sequentialIndex;
            }
            else
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            if (_questCompleted)
            {
                return;
            }

            _questCompleted = true;

            if (_definition.AutoCompleteMissionOnQuestEnd && GameManager.Instance != null && !string.IsNullOrWhiteSpace(_definition.QuestId))
            {
                GameManager.Instance.SetMissionFinished(_definition.QuestId);
            }
        }
    }
}
