namespace Synaptik.Gameplay.Quests
{
    /// <summary>
    /// Optional interface for components that expose a mission target when
    /// auto-completing quests. Implemented by gameplay actors such as aliens.
    /// </summary>
    public interface IQuestMissionTarget
    {
        AlienDefinition MissionAlienDefinition { get; }
    }
}
