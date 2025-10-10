using UnityEngine;

public interface IAlienReaction
{
    public void FeedbackColor(Color a_color);
    
    public void FeedbackAnimation(Animation a_animation);
    
    public void FeedbackTalking(string a_text);
}
