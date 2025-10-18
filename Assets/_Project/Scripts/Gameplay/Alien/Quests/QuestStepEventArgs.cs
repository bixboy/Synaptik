using System;

/// <summary>
/// Event payload emitted when quest steps change state.
/// </summary>
public readonly struct QuestStepEventArgs
{
    public QuestStepEventArgs(AlienQuestRuntime runtime, QuestStep step, QuestTrigger? trigger = null)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Step = step;
        Trigger = trigger;
    }

    public AlienQuestRuntime Runtime { get; }
    public QuestStep Step { get; }
    public QuestTrigger? Trigger { get; }

    public string QuestId => Runtime.QuestId;
    public string StepId => Step.StepId;
    public Alien Owner => Runtime.Owner;
    public bool HasStep => !string.IsNullOrWhiteSpace(Step.StepId);
}
