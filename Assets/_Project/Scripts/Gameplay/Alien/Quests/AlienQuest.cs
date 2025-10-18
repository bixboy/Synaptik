using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AlienQuest : IEquatable<AlienQuest>
{
    [SerializeField]
    private AlienDefinition alien;
    
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

    public AlienDefinition Alien => alien;
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
    
    public void SetAlien(AlienDefinition alienDefinition)
    {
        alien = alienDefinition;
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

    [Header("Trigger")]
    [SerializeField]
    private string customTriggerId;

    [SerializeField]
    private string[] additionalTriggerIds;

    [Header("Events")]
    [SerializeField]
    private QuestStepEvents events;

    public string StepId => stepId;
    public QuestStepType StepType => stepType;
    public bool CompletesQuest => completesQuest;
    public string NextStepId => nextStepId;
    public string CustomTriggerId => customTriggerId;
    public bool HasCustomTrigger => !string.IsNullOrWhiteSpace(customTriggerId);
    public IReadOnlyList<string> AdditionalTriggerIds => additionalTriggerIds ?? Array.Empty<string>();
    public QuestStepEvents Events => events ?? QuestStepEvents.Empty;

    public string PrimaryTriggerId => string.IsNullOrWhiteSpace(customTriggerId)
        ? QuestTriggerUtility.GetDefaultTriggerId(stepType)
        : customTriggerId;

    public string ResolveStepId(int index)
    {
        return string.IsNullOrWhiteSpace(stepId) ? $"step_{index}" : stepId;
    }

    public bool MatchesTrigger(in QuestTrigger trigger)
    {
        if (trigger.IsEmpty)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(trigger.TriggerId))
        {
            if (QuestTriggerUtility.EqualsTriggerIds(trigger.TriggerId, PrimaryTriggerId))
            {
                return true;
            }

            if (additionalTriggerIds != null)
            {
                for (int i = 0; i < additionalTriggerIds.Length; i++)
                {
                    if (QuestTriggerUtility.EqualsTriggerIds(trigger.TriggerId, additionalTriggerIds[i]))
                    {
                        return true;
                    }
                }
            }
        }

        if (trigger.LegacyType.HasValue && trigger.LegacyType.Value == stepType)
        {
            return true;
        }

        return false;
    }
}

public enum QuestStepType
{
    Talk = 0,
    GiveItem = 1
}
