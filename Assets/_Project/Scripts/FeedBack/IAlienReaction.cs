using UnityEngine;

namespace Synaptik.FeedBack
{
    public interface IAlienReaction
    {
        void FeedbackColor(Color color);
        void FeedbackAnimation(Animation animation);
        void FeedbackTalking(string text);
    }
}
