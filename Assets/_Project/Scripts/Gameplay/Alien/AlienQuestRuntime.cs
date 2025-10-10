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
            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            Debug.Log($"[QuestRuntime:{questId}] TryHandleStep trigger={triggerType} step='{stepId ?? "<current>"}' currentIndex={_currentIndex} completed={_questCompleted}");

            if (!_definition.HasSteps || _questCompleted)
            {
                Debug.Log($"[QuestRuntime:{questId}] Ignored trigger because quest has no steps or already completed.");
                return false;
            }

            if (string.IsNullOrEmpty(stepId))
            {
                Debug.Log($"[QuestRuntime:{questId}] No step id provided. Using current step.");
                return TryHandleCurrentStep(triggerType);
            }

            if (!_stepIndexById.TryGetValue(stepId, out var index))
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Step id '{stepId}' not found.");
                return false;
            }

            if (index < 0 || index >= _steps.Length)
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Step index {index} is out of range.");
                return false;
            }

            if (_steps[index].StepType != triggerType)
            {
                Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' expects {_steps[index].StepType} but received {triggerType}.");
                return false;
            }

            if (index != _currentIndex)
            {
                Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' is not the current step (current index {_currentIndex}).");
                return false;
            }

            ExecuteStep(index);
            return true;
        }

        public bool IsStepActive(string stepId, QuestStepType triggerType)
        {
            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            Debug.Log($"[QuestRuntime:{questId}] IsStepActive trigger={triggerType} step='{stepId ?? "<current>"}' currentIndex={_currentIndex} completed={_questCompleted}");

            if (!_definition.HasSteps || _questCompleted)
            {
                Debug.Log($"[QuestRuntime:{questId}] Quest has no steps or is completed. Returning inactive.");
                return false;
            }

            if (string.IsNullOrEmpty(stepId))
            {
                Debug.Log($"[QuestRuntime:{questId}] No step id provided. Checking current step only.");
                return IsCurrentStepActive(triggerType);
            }

            if (!_stepIndexById.TryGetValue(stepId, out var index))
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Step id '{stepId}' not found while checking activity.");
                return false;
            }

            if (index < 0 || index >= _steps.Length)
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Step index {index} out of range while checking activity.");
                return false;
            }

            if (_steps[index].StepType != triggerType)
            {
                Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' expects {_steps[index].StepType} but received {triggerType} while checking activity.");
                return false;
            }

            return index == _currentIndex;
        }

        public bool TryHandleCurrentStep(QuestStepType triggerType)
        {
            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            Debug.Log($"[QuestRuntime:{questId}] TryHandleCurrentStep trigger={triggerType} currentIndex={_currentIndex} completed={_questCompleted}");

            if (!_definition.HasSteps || _questCompleted)
            {
                Debug.Log($"[QuestRuntime:{questId}] Cannot handle current step because quest has no steps or already completed.");
                return false;
            }

            if (_currentIndex < 0 || _currentIndex >= _steps.Length)
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Current index {_currentIndex} out of range.");
                return false;
            }

            if (_steps[_currentIndex].StepType != triggerType)
            {
                Debug.Log($"[QuestRuntime:{questId}] Current step expects {_steps[_currentIndex].StepType} but received {triggerType}.");
                return false;
            }

            ExecuteStep(_currentIndex);
            return true;
        }

        public bool IsCurrentStepActive(QuestStepType triggerType)
        {
            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            Debug.Log($"[QuestRuntime:{questId}] IsCurrentStepActive trigger={triggerType} currentIndex={_currentIndex} completed={_questCompleted}");

            if (!_definition.HasSteps || _questCompleted)
            {
                Debug.Log($"[QuestRuntime:{questId}] Quest has no steps or already completed. Current step inactive.");
                return false;
            }

            if (_currentIndex < 0 || _currentIndex >= _steps.Length)
            {
                Debug.LogWarning($"[QuestRuntime:{questId}] Current index {_currentIndex} out of range while checking activity.");
                return false;
            }

            return _steps[_currentIndex].StepType == triggerType;
        }

        private void ExecuteStep(int index)
        {
            var step = _steps[index];

            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            Debug.Log($"[QuestRuntime:{questId}] Executing step index {index} completesQuest={step.CompletesQuest} nextStepId='{step.NextStepId}'.");

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
            var questId = _definition != null ? _definition.QuestId : "<unknown>";
            if (!string.IsNullOrEmpty(nextStepId) && _stepIndexById.TryGetValue(nextStepId, out var explicitIndex))
            {
                Debug.Log($"[QuestRuntime:{questId}] Advancing to explicit step id '{nextStepId}' at index {explicitIndex}.");
                _currentIndex = explicitIndex;
                return;
            }

            var sequentialIndex = currentIndex + 1;
            if (sequentialIndex < _steps.Length)
            {
                Debug.Log($"[QuestRuntime:{questId}] Advancing sequentially to index {sequentialIndex}.");
                _currentIndex = sequentialIndex;
            }
            else
            {
                Debug.Log($"[QuestRuntime:{questId}] No more steps. Completing quest.");
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            if (_questCompleted)
            {
                Debug.Log($"[QuestRuntime:{_definition?.QuestId ?? "<unknown>"}] CompleteQuest called but quest already completed.");
                return;
            }

            _questCompleted = true;

            Debug.Log($"[QuestRuntime:{_definition?.QuestId ?? "<unknown>"}] Quest completed.");

            if (_definition.AutoCompleteMissionOnQuestEnd && GameManager.Instance != null && !string.IsNullOrWhiteSpace(_definition.QuestId))
            {
                GameManager.Instance.SetMissionFinished(_definition.QuestId);
            }
        }
    }
}
