using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Game
{
    public readonly struct QuestStepRuntimeResult
    {
        public static readonly QuestStepRuntimeResult NotHandled = new QuestStepRuntimeResult(false, false);

        public QuestStepRuntimeResult(bool handled, bool overrideDefaultReaction)
        {
            Handled = handled;
            OverrideDefaultReaction = overrideDefaultReaction;
        }

        public bool Handled { get; }
        public bool OverrideDefaultReaction { get; }
    }

    public sealed class AlienQuestRuntime
    {
        private readonly Alien _owner;
        private readonly AlienQuest _definition;
        private readonly QuestStep[] _steps;
        private readonly string[] _stepIds;
        private readonly Dictionary<string, int> _stepIndexById;
        private readonly HashSet<string> _completedSteps = new HashSet<string>();
        private int _currentIndex;
        private bool _questCompleted;

        public AlienQuestRuntime(Alien owner, AlienQuest definition)
        {
            _owner = owner;
            _definition = definition;

            var steps = definition.Steps;
            _steps = new QuestStep[steps.Count];
            _stepIds = new string[steps.Count];
            _stepIndexById = new Dictionary<string, int>(steps.Count);

            for (int i = 0; i < steps.Count; i++)
            {
                _steps[i] = steps[i];
                var runtimeId = steps[i].ResolveStepId(i);
                _stepIds[i] = runtimeId;

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

        public QuestStepRuntimeResult TryHandleStep(string stepId)
        {
            if (!_definition.HasSteps || string.IsNullOrEmpty(stepId) || _questCompleted)
            {
                return QuestStepRuntimeResult.NotHandled;
            }

            if (!_stepIndexById.TryGetValue(stepId, out var index))
            {
                return QuestStepRuntimeResult.NotHandled;
            }

            var runtimeId = _stepIds[index];
            if (!_steps[index].AllowMultipleTriggers && _completedSteps.Contains(runtimeId))
            {
                return QuestStepRuntimeResult.NotHandled;
            }

            if (_definition.EnforceStepOrder && index != _currentIndex)
            {
                return QuestStepRuntimeResult.NotHandled;
            }

            ExecuteStep(index);

            return new QuestStepRuntimeResult(true, _steps[index].OverrideDefaultReaction);
        }

        private void ExecuteStep(int index)
        {
            var step = _steps[index];
            var runtimeId = _stepIds[index];

            foreach (var action in step.Actions)
            {
                ExecuteAction(action);
            }

            if (!step.AllowMultipleTriggers)
            {
                _completedSteps.Add(runtimeId);
            }

            if (step.CompletesQuest)
            {
                CompleteQuest();
            }
            else if (step.AutoAdvanceToNext)
            {
                AdvanceToNext(index, step.NextStepId);
            }
        }

        private void ExecuteAction(QuestAction action)
        {
            switch (action.ActionType)
            {
                case QuestActionType.ChangeEmotion:
                    _owner.SetEmotion(action.EmotionValue);
                    break;
                case QuestActionType.AdjustMistrust:
                    if (action.IntValue != 0 && MistrustManager.Instance != null)
                    {
                        if (action.IntValue >= 0)
                        {
                            MistrustManager.Instance.AddMistrust(action.IntValue);
                        }
                        else
                        {
                            MistrustManager.Instance.RemoveMistrust(-action.IntValue);
                        }
                    }
                    break;
                case QuestActionType.ShowDialogue:
                    if (!string.IsNullOrWhiteSpace(action.DialogueLine))
                    {
                        _owner.ShowDialogue(action.DialogueLine, action.DialogueDuration);
                    }
                    break;
                case QuestActionType.RegisterMission:
                    RegisterMission(action);
                    break;
                case QuestActionType.CompleteMission:
                    CompleteMission(action);
                    break;
            }
        }

        private void RegisterMission(QuestAction action)
        {
            if (GameManager.Instance == null || string.IsNullOrWhiteSpace(action.MissionId))
            {
                return;
            }

            var title = string.IsNullOrWhiteSpace(action.MissionTitle) ? action.MissionId : action.MissionTitle;
            var description = action.MissionDescription;
            GameManager.Instance.RegisterMission(new Mission(action.MissionId, title, description));
        }

        private void CompleteMission(QuestAction action)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            var missionId = string.IsNullOrWhiteSpace(action.MissionId) ? _definition.QuestId : action.MissionId;
            if (!string.IsNullOrWhiteSpace(missionId))
            {
                GameManager.Instance.SetMissionFinished(missionId);
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
            else if (_definition.AutoCompleteOnLastStep)
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
