
using Synaptik.Game;

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
    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null);
}
