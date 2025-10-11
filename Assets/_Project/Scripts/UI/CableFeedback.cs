using UnityEngine;
using UnityEngine.UI;

public sealed class CableFeedback : MonoBehaviour
{
    [Header("Output References")]
    [SerializeField]
    private Image outputLeft;

    [SerializeField]
    private Image outputRight;

    [Header("Action Colors")]
    [SerializeField]
    private Color defaultActionColor = Color.white;

    [SerializeField]
    private Color talkColor;

    [SerializeField]
    private Color actionColor;

    [Header("Emotion Colors")]
    [SerializeField]
    private Color defaultEmotionColor = Color.white;

    [SerializeField]
    private Color curiousColor;

    [SerializeField]
    private Color angryColor;

    [SerializeField]
    private Color fearfulColor;

    [SerializeField]
    private Color friendlyColor;

    private void Start()
    {
        if (Core.InputsDetection.Instance == null)
        {
            return;
        }

        Core.InputsDetection.Instance.OnEmotion += HandleEmotion;
        Core.InputsDetection.Instance.OnAction += HandleAction;
    }

    private void OnDestroy()
    {
        if (Core.InputsDetection.Instance == null)
        {
            return;
        }

        Core.InputsDetection.Instance.OnEmotion -= HandleEmotion;
        Core.InputsDetection.Instance.OnAction -= HandleAction;
    }

    private void HandleEmotion(Emotion emotion, bool keyReleased)
    {
        if (outputRight == null)
        {
            return;
        }

        outputRight.color = keyReleased ? defaultEmotionColor : GetEmotionColor(emotion);
    }

    private void HandleAction(Behavior behavior, bool keyReleased)
    {
        if (outputLeft == null)
        {
            return;
        }

        outputLeft.color = keyReleased ? defaultActionColor : GetActionColor(behavior);
    }

    private Color GetEmotionColor(Emotion emotion)
    {
        return emotion switch
        {
            Emotion.Anger => angryColor,
            Emotion.Curious => curiousColor,
            Emotion.Fearful => fearfulColor,
            Emotion.Friendly => friendlyColor,
            _ => defaultEmotionColor
        };
    }

    private Color GetActionColor(Behavior behavior)
    {
        return behavior switch
        {
            Behavior.Action => actionColor,
            Behavior.Talking => talkColor,
            _ => defaultActionColor
        };
    }
}
