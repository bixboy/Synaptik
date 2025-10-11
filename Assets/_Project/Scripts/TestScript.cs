using UnityEngine;

namespace Synaptik
{
    public sealed class TestScript : MonoBehaviour
    {
        private void Start()
        {
            const string text = "Hello World!";

            Debug.Log(Core.TextFeedBack.ProgressiveDisplayLerp(text, 0.0f, '-'));
            Debug.Log(Core.TextFeedBack.ProgressiveDisplayLerp(text, 0.5f, '_'));
            Debug.Log(Core.TextFeedBack.ProgressiveDisplayLerp(text, 0.867f, '='));
            Debug.Log(Core.TextFeedBack.ProgressiveDisplayLerp(text, 1.0f));

            Core.TextFeedBack.ProgressiveDisplayTimeSpacing(text, 0.1f, 1);
        }
    }
}
