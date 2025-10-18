using System;

/// <summary>
/// Helper utilities for normalising and comparing quest trigger identifiers.
/// </summary>
public static class QuestTriggerUtility
{
    /// <summary>
    /// Normalises a trigger identifier for deterministic comparisons.
    /// </summary>
    public static string NormalizeTriggerId(string triggerId)
    {
        return string.IsNullOrWhiteSpace(triggerId)
            ? string.Empty
            : triggerId.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Returns true when the provided candidate matches the normalised trigger id.
    /// </summary>
    public static bool EqualsTriggerIds(string normalizedTriggerId, string candidate)
    {
        if (string.IsNullOrEmpty(normalizedTriggerId))
        {
            return string.IsNullOrWhiteSpace(candidate);
        }

        return normalizedTriggerId == NormalizeTriggerId(candidate);
    }

    /// <summary>
    /// Provides a default trigger identifier for legacy quest step types.
    /// </summary>
    public static string GetDefaultTriggerId(QuestStepType stepType)
    {
        return stepType switch
        {
            QuestStepType.Talk => "talk",
            QuestStepType.GiveItem => "give_item",
            _ => stepType.ToString().ToLowerInvariant()
        };
    }
}
