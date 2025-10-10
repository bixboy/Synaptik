using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct FeedBack
{
    public Emotion emotion;
    
    public Color emotionColor;
    //public Animation animationReaction;
    public string talkingReaction;
}

public class AlienFeedBack : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    
    [Header("Feedbacks")]
    [SerializeField] private List<FeedBack> _feedbackList;
    
    private Dictionary<Emotion, Color> _feedbackColor = new Dictionary<Emotion, Color>();
    //private Dictionary<Emotion, Animation> _feedbackAnimation = new Dictionary<Emotion, Animation>();
    private Dictionary<Emotion, string> _feedbackTalking= new Dictionary<Emotion, string>();

    private void Start()
    {
        for (int i = 0; i < _feedbackList.Count; i++)
        {
            _feedbackColor.Add(_feedbackList[i].emotion, _feedbackList[i].emotionColor);
            //_feedbackAnimation.Add(_feedbackList[i].emotion, _feedbackList[i].animationReaction);
            _feedbackTalking.Add(_feedbackList[i].emotion, _feedbackList[i].talkingReaction);
        }
    }

    private void ActionFeedback(IAlienReaction a_alien, Emotion a_emotion, Behavior a_behavior)
    {
        if (a_alien == null)
            return;
        
        a_alien.FeedbackColor(_feedbackColor[a_emotion]);
        
        switch (a_behavior)
        {
            case Behavior.Action:
                //a_alien.FeedbackAnimation(_feedbackAnimation[a_emotion]);
                break;
            case Behavior.Talking:
                a_alien.FeedbackTalking(_feedbackTalking[a_emotion]);
                break;
            
            default:
                break;
        }
    }
}
