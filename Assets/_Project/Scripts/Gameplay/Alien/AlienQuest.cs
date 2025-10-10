using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Game
{
    [Serializable]
    public struct AlienQuest
    {
        [SerializeField] private string _questId;
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private bool _autoRegisterMission;
        [SerializeField] private bool _autoCompleteMissionOnQuestEnd;
        [SerializeField] private QuestStep[] _steps;

        public string QuestId => _questId;
        public string Title => _title;
        public string Description => _description;
        public bool AutoRegisterMission => _autoRegisterMission;
        public bool AutoCompleteMissionOnQuestEnd => _autoCompleteMissionOnQuestEnd;
        public IReadOnlyList<QuestStep> Steps => _steps ?? Array.Empty<QuestStep>();
        public bool HasSteps => _steps != null && _steps.Length > 0;

        public QuestStep GetStepAt(int index)
        {
            return _steps != null && index >= 0 && index < _steps.Length ? _steps[index] : default;
        }
    }

    [Serializable]
    public struct QuestStep
    {
        [SerializeField] private string _stepId;
        [SerializeField] private QuestStepType _stepType;
        [SerializeField] private bool _completesQuest;
        [SerializeField] private string _nextStepId;

        public string StepId => _stepId;
        public QuestStepType StepType => _stepType;
        public bool CompletesQuest => _completesQuest;
        public string NextStepId => _nextStepId;

        public string ResolveStepId(int index)
        {
            return string.IsNullOrWhiteSpace(_stepId) ? $"step_{index}" : _stepId;
        }
    }

    public enum QuestStepType
    {
        Talk = 0,
        GiveItem = 1
    }
}
