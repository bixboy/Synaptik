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
    private Color curiousColor = new Color(127, 213, 93, 255);

    [SerializeField]
    private Color angryColor = new Color(240, 83, 83, 255);

    [SerializeField]
    private Color fearfulColor = new Color(15, 192, 222, 255);

    [SerializeField]
    private Color friendlyColor = new Color(255, 221, 97, 255);

    private void Start()
    {
        if (InputsDetection.Instance == null)
        {
            return;
        }

        InputsDetection.Instance.OnEmotion += HandleEmotion;
        InputsDetection.Instance.OnAction += HandleAction;
    }

    private void OnDestroy()
    {
        if (InputsDetection.Instance == null)
        {
            return;
        }

        InputsDetection.Instance.OnEmotion -= HandleEmotion;
        InputsDetection.Instance.OnAction -= HandleAction;
    }

    private void HandleEmotion(Emotion emotion, bool keyReleased)
    {
        if (!outputRight)
            return;

        outputRight.color = keyReleased ? defaultEmotionColor : GetEmotionColor(emotion);
    }

    private void HandleAction(Behavior behavior, bool keyReleased)
    {
        if (!outputLeft)
            return;

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
