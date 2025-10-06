using UnityEngine;

[System.Serializable]
public enum Emotion
{
  None,
  Anger,
  Friendly,
  Sad,
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
}

public interface IInteraction
{
    public void Interact(ActionValues _action);

    public void RegisterMission();
}
