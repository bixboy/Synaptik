
[System.Serializable]
public enum Emotion
{
  None,
  Anger,
  Friendly,
  Curious,
  Fearful
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
