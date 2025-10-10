
[System.Serializable]
public enum Emotion
{
  None = 0,
  Anger = 1,
  Friendly = 2,
  Curious = 3,
  Fearful = 4
}
[System.Serializable]
public enum Behavior
{
    None,
    Talking,
    Action
}


[System.Serializable]
public enum AlienVerb
{
    Ask,
    Compliment,
    Insult,
    Yield,
    Hit,
    Give,
}
[System.Serializable]
public struct ActionValues
{
    public Emotion _emotion;
    public Behavior _behavior;
}

public interface IInteraction
{
    public void Interact(ActionValues _action);

    public void RegisterMission();
}
