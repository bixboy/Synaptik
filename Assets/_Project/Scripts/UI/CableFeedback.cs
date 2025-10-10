using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CableFeedback : MonoBehaviour
{
    [Header("Output References")]
    [SerializeField] private Image _outputLeft;
    [SerializeField] private Image _outputRight;

    [Header("Action Colors")]
    [SerializeField] private Color _defaultActionColor = Color.white;
    [SerializeField] private Color _talkColor;
    [SerializeField] private Color _actionColor;

    [Header("Emotion Colors")]
    [SerializeField] private Color _defaultEmotionColor = Color.white;
    [SerializeField] private Color _curiousColor;
    [SerializeField] private Color _angryColor;
    [SerializeField] private Color _fearfulColor;
    [SerializeField] private Color _friendlyColor;

    private void Start()
    {
        if (InputsDetection.Instance)
        {
            InputsDetection.Instance.OnEmotion += HandleEmotion;
            InputsDetection.Instance.OnAction += HandleAction;
        }
    }

    private void OnDestroy()
    {
        if (InputsDetection.Instance)
        {
            InputsDetection.Instance.OnEmotion -= HandleEmotion;
            InputsDetection.Instance.OnAction -= HandleAction;
        }
    }

    private void HandleEmotion(Emotion emotion, bool value)
    {
        if (!_outputRight)
            return;

        if (value)
        {
            _outputRight.color = _defaultEmotionColor;
            return;
        }

        switch (emotion)
        {
            case Emotion.Anger:
                _outputRight.color = _angryColor;
                break;
            
            case Emotion.Curious:
                _outputRight.color = _curiousColor;
                break;
            
            case Emotion.Fearful:
                _outputRight.color = _fearfulColor;
                break;
            
            case Emotion.Friendly:
                _outputRight.color = _friendlyColor;
                break;
            
            default:
                _outputRight.color = _defaultEmotionColor;
                break;
        }
    }

    private void HandleAction(Behavior behavior, bool value)
    {
        if (!_outputLeft)
            return;

        if (value)
        {
            _outputLeft.color = _defaultActionColor;
            return;
        }

        switch (behavior)
        {
            case Behavior.Action:
                _outputLeft.color = _actionColor;
                break;
            
            case Behavior.Talking:
                _outputLeft.color = _talkColor;
                break;
            
            default:
                _outputLeft.color = _defaultActionColor;
                break;
        }
    }
}
