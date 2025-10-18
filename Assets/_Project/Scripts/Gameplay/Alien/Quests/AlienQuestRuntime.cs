using System.Collections.Generic;
using UnityEngine;

public sealed class AlienQuestRuntime
{
    private readonly AlienQuest _definition;
    private readonly QuestStep[] _steps;
    private readonly Dictionary<string, int> _stepIndexById;
    private readonly Alien _owner;
    private int _currentIndex;
    private bool _questCompleted;
    private bool _initialized;

    public AlienQuestRuntime(AlienQuest definition, Alien owner)
    {
        _definition = definition;
        _owner = owner;

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

    public Alien Owner => _owner;
    public string QuestId => _definition.Equals(default(AlienQuest)) ? string.Empty : _definition.QuestId;
    public bool IsCompleted => _questCompleted;

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        if (_definition.HasSteps && !_questCompleted)
        {
            ActivateCurrentStep();
        }
    }

    public bool TryGetCurrentStep(out QuestStep step)
    {
        if (_definition.HasSteps && !_questCompleted && _currentIndex >= 0 && _currentIndex < _steps.Length)
        {
            step = _steps[_currentIndex];
            return true;
        }

        step = default;
        return false;
    }

    public bool TryHandleStep(string stepId, QuestStepType triggerType)
    {
        return TryHandleTrigger(stepId, QuestTrigger.FromLegacy(triggerType));
    }

    public bool TryHandleTrigger(QuestTrigger trigger)
    {
        return TryHandleTrigger(null, trigger);
    }

    public bool TryHandleTrigger(string stepId, QuestTrigger trigger)
    {
        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        Debug.Log($"[QuestRuntime:{questId}] TryHandleTrigger trigger={trigger} step='{stepId ?? "<current>"}' currentIndex={_currentIndex} completed={_questCompleted}");

        if (!_definition.HasSteps || _questCompleted)
        {
            Debug.Log($"[QuestRuntime:{questId}] Ignored trigger because quest has no steps or already completed.");
            return false;
        }

        if (trigger.IsEmpty)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Received empty trigger for step '{stepId ?? "<current>"}'.");
            return false;
        }

        if (string.IsNullOrEmpty(stepId))
        {
            Debug.Log($"[QuestRuntime:{questId}] No step id provided. Using current step.");
            return TryHandleCurrentStep(trigger);
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

        if (!_steps[index].MatchesTrigger(trigger))
        {
            Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' does not accept trigger {trigger}.");
            return false;
        }

        if (index != _currentIndex)
        {
            Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' is not the current step (current index {_currentIndex}).");
            return false;
        }

        ExecuteStep(index, trigger);
        return true;
    }

    public bool IsStepActive(string stepId, QuestStepType triggerType)
    {
        return IsStepActive(stepId, QuestTrigger.FromLegacy(triggerType));
    }

    public bool IsStepActive(string stepId, QuestTrigger trigger)
    {
        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        Debug.Log($"[QuestRuntime:{questId}] IsStepActive trigger={trigger} step='{stepId ?? "<current>"}' currentIndex={_currentIndex} completed={_questCompleted}");

        if (!_definition.HasSteps || _questCompleted)
        {
            Debug.Log($"[QuestRuntime:{questId}] Quest has no steps or is completed. Returning inactive.");
            return false;
        }

        if (trigger.IsEmpty)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Cannot evaluate activity with an empty trigger.");
            return false;
        }

        if (string.IsNullOrEmpty(stepId))
        {
            Debug.Log($"[QuestRuntime:{questId}] No step id provided. Checking current step only.");
            return IsCurrentStepActive(trigger);
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

        if (!_steps[index].MatchesTrigger(trigger))
        {
            Debug.Log($"[QuestRuntime:{questId}] Step '{stepId}' does not accept trigger {trigger} while checking activity.");
            return false;
        }

        return index == _currentIndex;
    }

    public bool TryHandleCurrentStep(QuestStepType triggerType)
    {
        return TryHandleCurrentStep(QuestTrigger.FromLegacy(triggerType));
    }

    public bool IsStepActive(QuestTrigger trigger)
    {
        return IsStepActive(null, trigger);
    }

    public bool TryHandleCurrentStep(QuestTrigger trigger)
    {
        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        Debug.Log($"[QuestRuntime:{questId}] TryHandleCurrentStep trigger={trigger} currentIndex={_currentIndex} completed={_questCompleted}");

        if (!_definition.HasSteps || _questCompleted)
        {
            Debug.Log($"[QuestRuntime:{questId}] Cannot handle current step because quest has no steps or already completed.");
            return false;
        }

        if (trigger.IsEmpty)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Cannot handle current step with an empty trigger.");
            return false;
        }

        if (_currentIndex < 0 || _currentIndex >= _steps.Length)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Current index {_currentIndex} out of range.");
            return false;
        }

        if (!_steps[_currentIndex].MatchesTrigger(trigger))
        {
            Debug.Log($"[QuestRuntime:{questId}] Current step does not accept trigger {trigger}.");
            return false;
        }

        ExecuteStep(_currentIndex, trigger);
        return true;
    }

    public bool IsCurrentStepActive(QuestStepType triggerType)
    {
        return IsCurrentStepActive(QuestTrigger.FromLegacy(triggerType));
    }

    public bool IsCurrentStepActive(QuestTrigger trigger)
    {
        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        Debug.Log($"[QuestRuntime:{questId}] IsCurrentStepActive trigger={trigger} currentIndex={_currentIndex} completed={_questCompleted}");

        if (!_definition.HasSteps || _questCompleted)
        {
            Debug.Log($"[QuestRuntime:{questId}] Quest has no steps or already completed. Current step inactive.");
            return false;
        }

        if (trigger.IsEmpty)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Cannot evaluate current step with an empty trigger.");
            return false;
        }

        if (_currentIndex < 0 || _currentIndex >= _steps.Length)
        {
            Debug.LogWarning($"[QuestRuntime:{questId}] Current index {_currentIndex} out of range while checking activity.");
            return false;
        }

        return _steps[_currentIndex].MatchesTrigger(trigger);
    }

    private void ExecuteStep(int index, in QuestTrigger trigger)
    {
        var step = _steps[index];

        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        Debug.Log($"[QuestRuntime:{questId}] Executing step index {index} completesQuest={step.CompletesQuest} nextStepId='{step.NextStepId}'.");

        step.Events.InvokeCompleted();
        QuestRuntimeRegistry.NotifyStepCompleted(new QuestStepEventArgs(this, step, trigger));

        if (step.CompletesQuest)
        {
            CompleteQuest(trigger, step);
        }
        else
        {
            AdvanceToNext(index, step.NextStepId, step);
        }
    }

    private void AdvanceToNext(int currentIndex, string nextStepId, QuestStep completedStep)
    {
        if (_questCompleted)
        {
            return;
        }

        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;
        if (!string.IsNullOrEmpty(nextStepId) && _stepIndexById.TryGetValue(nextStepId, out var explicitIndex))
        {
            Debug.Log($"[QuestRuntime:{questId}] Advancing to explicit step id '{nextStepId}' at index {explicitIndex}.");
            _currentIndex = explicitIndex;
            ActivateCurrentStep();
            return;
        }

        var sequentialIndex = currentIndex + 1;
        if (sequentialIndex < _steps.Length)
        {
            Debug.Log($"[QuestRuntime:{questId}] Advancing sequentially to index {sequentialIndex}.");
            _currentIndex = sequentialIndex;
            ActivateCurrentStep();
        }
        else
        {
            Debug.Log($"[QuestRuntime:{questId}] No more steps. Completing quest.");
            CompleteQuest(null, completedStep);
        }
    }

    private void ActivateCurrentStep()
    {
        if (_questCompleted || !_definition.HasSteps)
        {
            return;
        }

        if (_currentIndex < 0 || _currentIndex >= _steps.Length)
        {
            Debug.LogWarning($"[QuestRuntime:{QuestId}] Current index {_currentIndex} out of range while activating step.");
            return;
        }

        var step = _steps[_currentIndex];
        step.Events.InvokeActivated();
        QuestRuntimeRegistry.NotifyStepActivated(new QuestStepEventArgs(this, step));
    }

    private void CompleteQuest(QuestTrigger? trigger, QuestStep? completedStep)
    {
        var questId = string.IsNullOrWhiteSpace(QuestId) ? "<unknown>" : QuestId;

        if (_questCompleted)
        {
            Debug.Log($"[QuestRuntime:{questId}] CompleteQuest called but quest already completed.");
            return;
        }

        _questCompleted = true;

        Debug.Log($"[QuestRuntime:{questId}] Quest completed.");

        QuestRuntimeRegistry.NotifyQuestCompleted(new QuestStepEventArgs(this, completedStep ?? default, trigger));

        if (_definition.AutoCompleteMissionOnQuestEnd && GameManager.Instance != null && !string.IsNullOrWhiteSpace(_definition.QuestId))
        {
            GameManager.Instance.SetMissionFinished(_definition.QuestId, _definition.Alien);
        }
    }
}
