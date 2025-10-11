using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synaptik.Gameplay.Alien
{
    [Serializable]
    public struct AlienQuest : IEquatable<AlienQuest>
    {
        [SerializeField]
        private string questId;

        [SerializeField]
        private string title;

        [SerializeField, TextArea]
        private string description;

        [SerializeField]
        private bool autoRegisterMission;

        [SerializeField]
        private bool autoCompleteMissionOnQuestEnd;

        [SerializeField]
        private QuestStep[] steps;

        public string QuestId => questId;
        public string Title => title;
        public string Description => description;
        public bool AutoRegisterMission => autoRegisterMission;
        public bool AutoCompleteMissionOnQuestEnd => autoCompleteMissionOnQuestEnd;
        public IReadOnlyList<QuestStep> Steps => steps ?? Array.Empty<QuestStep>();
        public bool HasSteps => steps is { Length: > 0 };

        public QuestStep GetStepAt(int index)
        {
            return steps != null && index >= 0 && index < steps.Length ? steps[index] : default;
        }

        public bool Equals(AlienQuest other)
        {
            return questId == other.questId &&
                   title == other.title &&
                   description == other.description &&
                   autoRegisterMission == other.autoRegisterMission &&
                   autoCompleteMissionOnQuestEnd == other.autoCompleteMissionOnQuestEnd &&
                   Equals(steps, other.steps);
        }

        public override bool Equals(object obj)
        {
            return obj is AlienQuest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(questId, title, description, autoRegisterMission, autoCompleteMissionOnQuestEnd, steps);
        }
    }

    [Serializable]
    public struct QuestStep
    {
        [SerializeField]
        private string stepId;

        [SerializeField]
        private QuestStepType stepType;

        [SerializeField]
        private bool completesQuest;

        [SerializeField]
        private string nextStepId;

        public string StepId => stepId;
        public QuestStepType StepType => stepType;
        public bool CompletesQuest => completesQuest;
        public string NextStepId => nextStepId;

        public string ResolveStepId(int index)
        {
            return string.IsNullOrWhiteSpace(stepId) ? $"step_{index}" : stepId;
        }
    }

    public enum QuestStepType
    {
        Talk = 0,
        GiveItem = 1
    }
}
