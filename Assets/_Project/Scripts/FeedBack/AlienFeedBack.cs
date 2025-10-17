using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct FeedBack
{
    public Emotion emotion;
    public Color emotionColor;
    public string talkingReaction;
}

public sealed class AlienFeedBack : MonoBehaviour
{
    [Header("Feedbacks")]
    [SerializeField]
    private List<FeedBack> feedbackList = new();

    private readonly Dictionary<Emotion, Color> feedbackColors = new();
    private readonly Dictionary<Emotion, string> feedbackTalking = new();

    private void Start()
    {
        foreach (var feedback in feedbackList)
        {
            feedbackColors[feedback.emotion] = feedback.emotionColor;
            feedbackTalking[feedback.emotion] = feedback.talkingReaction;
        }
    }

    private void ActionFeedback(IAlienReaction alien, Emotion emotion, Behavior behavior)
    {
        if (alien == null)
        {
            return;
        }

        if (feedbackColors.TryGetValue(emotion, out var color))
        {
            alien.FeedbackColor(color);
        }

        if (behavior == Behavior.Talking && feedbackTalking.TryGetValue(emotion, out var reaction))
        {
            alien.FeedbackTalking(reaction);
        }
    }
}
