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
        [SerializeField] private bool _autoRegisterMission = true;
        [SerializeField] private bool _autoCompleteMissionOnQuestEnd = true;
        [SerializeField] private bool _enforceStepOrder = true;
        [SerializeField] private bool _autoCompleteOnLastStep = true;
        [SerializeField] private QuestStep[] _steps;

        public string QuestId => _questId;
        public string Title => _title;
        public string Description => _description;
        public bool AutoRegisterMission => _autoRegisterMission;
        public bool AutoCompleteMissionOnQuestEnd => _autoCompleteMissionOnQuestEnd;
        public bool EnforceStepOrder => _enforceStepOrder;
        public bool AutoCompleteOnLastStep => _autoCompleteOnLastStep;
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
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private bool _allowMultipleTriggers;
        [SerializeField] private bool _autoAdvanceToNext = true;
        [SerializeField] private string _nextStepId;
        [SerializeField] private bool _overrideDefaultReaction;
        [SerializeField] private bool _completesQuest;
        [SerializeField] private QuestAction[] _actions;

        public string StepId => _stepId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public bool AllowMultipleTriggers => _allowMultipleTriggers;
        public bool AutoAdvanceToNext => _autoAdvanceToNext;
        public string NextStepId => _nextStepId;
        public bool OverrideDefaultReaction => _overrideDefaultReaction;
        public bool CompletesQuest => _completesQuest;
        public IReadOnlyList<QuestAction> Actions => _actions ?? Array.Empty<QuestAction>();

        public string ResolveStepId(int index)
        {
            return string.IsNullOrWhiteSpace(_stepId) ? $"step_{index}" : _stepId;
        }
    }

    public enum QuestActionType
    {
        ChangeEmotion,
        AdjustMistrust,
        ShowDialogue,
        RegisterMission,
        CompleteMission
    }

    [Serializable]
    public struct QuestAction
    {
        [SerializeField] private QuestActionType _actionType;
        [SerializeField] private Emotion _emotion;
        [SerializeField] private int _intValue;
        [SerializeField] private string _missionId;
        [SerializeField] private string _missionTitle;
        [SerializeField, TextArea] private string _missionDescription;
        [SerializeField, TextArea] private string _dialogueLine;
        [SerializeField] private float _dialogueDuration;

        public QuestActionType ActionType => _actionType;
        public Emotion EmotionValue => _emotion;
        public int IntValue => _intValue;
        public string MissionId => _missionId;
        public string MissionTitle => _missionTitle;
        public string MissionDescription => _missionDescription;
        public string DialogueLine => _dialogueLine;
        public float DialogueDuration => _dialogueDuration <= 0f ? 2f : _dialogueDuration;
    }
}
