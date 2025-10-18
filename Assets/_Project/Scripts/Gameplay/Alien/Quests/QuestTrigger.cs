/// <summary>
/// Represents a gameplay event that can progress a quest step.
/// </summary>
public readonly struct QuestTrigger
{
    public QuestTrigger(string triggerId)
    {
        TriggerId = QuestTriggerUtility.NormalizeTriggerId(triggerId);
        LegacyType = null;
        Payload = null;
    }

    public QuestTrigger(string triggerId, object payload)
    {
        TriggerId = QuestTriggerUtility.NormalizeTriggerId(triggerId);
        LegacyType = null;
        Payload = payload;
    }

    public QuestTrigger(QuestStepType legacyType)
    {
        TriggerId = QuestTriggerUtility.GetDefaultTriggerId(legacyType);
        LegacyType = legacyType;
        Payload = null;
    }

    public QuestTrigger(string triggerId, QuestStepType? legacyType, object payload)
    {
        TriggerId = QuestTriggerUtility.NormalizeTriggerId(triggerId);
        LegacyType = legacyType;
        Payload = payload;
    }

    public string TriggerId { get; }
    public QuestStepType? LegacyType { get; }
    public object Payload { get; }

    public bool IsEmpty => string.IsNullOrEmpty(TriggerId) && !LegacyType.HasValue;

    public static QuestTrigger FromLegacy(QuestStepType legacyType)
    {
        return new QuestTrigger(QuestTriggerUtility.GetDefaultTriggerId(legacyType), legacyType, null);
    }

    public QuestTrigger WithPayload(object payload)
    {
        return new QuestTrigger(TriggerId, LegacyType, payload);
    }

    public override string ToString()
    {
        return $"TriggerId='{TriggerId}' LegacyType={(LegacyType.HasValue ? LegacyType.Value.ToString() : "<none>")}";
    }
}
