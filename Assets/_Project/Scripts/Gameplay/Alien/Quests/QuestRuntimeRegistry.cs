using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global registry exposing quest runtimes so that gameplay systems can raise triggers
/// and observe lifecycle changes without tight coupling to alien components.
/// </summary>
public static class QuestRuntimeRegistry
{
    private static readonly Dictionary<string, AlienQuestRuntime> Runtimes = new();

    /// <summary>
    /// Raised when a quest step becomes active for a registered runtime.
    /// </summary>
    public static event System.Action<QuestStepEventArgs> StepActivated;

    /// <summary>
    /// Raised when a quest step completes successfully.
    /// </summary>
    public static event System.Action<QuestStepEventArgs> StepCompleted;

    /// <summary>
    /// Raised when a quest finishes. The <see cref="QuestStepEventArgs.HasStep"/> flag indicates
    /// whether a specific step was responsible for completion.
    /// </summary>
    public static event System.Action<QuestStepEventArgs> QuestCompleted;

    public static void Register(AlienQuestRuntime runtime)
    {
        if (runtime == null || string.IsNullOrWhiteSpace(runtime.QuestId))
        {
            return;
        }

        if (Runtimes.TryGetValue(runtime.QuestId, out var existing) && existing != runtime)
        {
            Debug.LogWarning($"[QuestRegistry] Overwriting existing runtime for quest '{runtime.QuestId}'.");
        }

        Runtimes[runtime.QuestId] = runtime;
    }

    public static void Unregister(AlienQuestRuntime runtime)
    {
        if (runtime == null || string.IsNullOrWhiteSpace(runtime.QuestId))
        {
            return;
        }

        if (Runtimes.TryGetValue(runtime.QuestId, out var existing) && existing == runtime)
        {
            Runtimes.Remove(runtime.QuestId);
        }
    }

    public static bool RaiseTrigger(string questId, QuestTrigger trigger)
    {
        return RaiseTrigger(questId, null, trigger);
    }

    public static bool RaiseTrigger(string questId, string stepId, QuestTrigger trigger)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            Debug.LogWarning($"[QuestRegistry] Tried to raise trigger without quest id. Trigger: {trigger}.");
            return false;
        }

        if (!Runtimes.TryGetValue(questId, out var runtime))
        {
            Debug.LogWarning($"[QuestRegistry] No runtime registered for quest '{questId}'. Trigger: {trigger}.");
            return false;
        }

        return runtime.TryHandleTrigger(stepId, trigger);
    }

    internal static void NotifyStepActivated(in QuestStepEventArgs args)
    {
        StepActivated?.Invoke(args);
    }

    internal static void NotifyStepCompleted(in QuestStepEventArgs args)
    {
        StepCompleted?.Invoke(args);
    }

    internal static void NotifyQuestCompleted(in QuestStepEventArgs args)
    {
        QuestCompleted?.Invoke(args);
    }
}
