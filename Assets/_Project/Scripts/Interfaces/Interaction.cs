using UnityEngine;

public enum Emotion
{
  None,
  Anger,
  Friendly,
  Sad,
  Curious,
  Fearful
}

public enum Behavior
{
    None,
    Talking,
    Action
}

public struct ActionValues
{
    
}

public interface IInteraction
{
    ActionValues Values { get; }
}
