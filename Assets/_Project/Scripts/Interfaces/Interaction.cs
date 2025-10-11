
public enum Emotion
{
    None = 0,
    Anger = 1,
    Friendly = 2,
    Curious = 3,
    Fearful = 4
}

public enum Behavior
{
    None,
    Talking,
    Action
}

[System.Serializable]
public struct ActionValues
{
    public Emotion _emotion;
    public Behavior _behavior;

    public ActionValues(Emotion emotion, Behavior behavior)
    {
        _emotion = emotion;
        _behavior = behavior;
    }
}

public interface IInteraction
{
    void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null);
}
