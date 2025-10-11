using UnityEngine;

public sealed class TestScript : MonoBehaviour
{
    private void Start()
    {
        const string text = "Hello World!";

        Debug.Log(TextFeedBack.ProgressiveDisplayLerp(text, 0.0f, '-'));
        Debug.Log(TextFeedBack.ProgressiveDisplayLerp(text, 0.5f, '_'));
        Debug.Log(TextFeedBack.ProgressiveDisplayLerp(text, 0.867f, '='));
        Debug.Log(TextFeedBack.ProgressiveDisplayLerp(text, 1.0f));

        TextFeedBack.ProgressiveDisplayTimeSpacing(text, 0.1f, 1);
    }
}
