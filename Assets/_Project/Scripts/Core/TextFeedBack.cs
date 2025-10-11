using System.Text;
using UnityEngine;

namespace Synaptik.Core
{
    public static class TextFeedBack
    {
        public static string ProgressiveDisplayLerp(string text, float lerp)
        {
            return text[..ClampIndex(text.Length, text.Length * lerp)];
        }

        public static string ProgressiveDisplayLerp(string text, float lerp, char filler)
        {
            return FillWith(text, ClampIndex(text.Length, text.Length * lerp), filler);
        }

        public static string ProgressiveDisplayTimeSpacing(string text, float spacingTime, float elapsedTime)
        {
            return text[..ClampIndex(text.Length, elapsedTime / spacingTime)];
        }

        public static string ProgressiveDisplayTimeSpacing(string text, float spacingTime, float elapsedTime, char filler)
        {
            return FillWith(text, ClampIndex(text.Length, elapsedTime / spacingTime), filler);
        }

        private static int ClampIndex(int length, float rawIndex)
        {
            return Mathf.Clamp((int)rawIndex, 0, length);
        }

        private static string FillWith(string source, int visibleLength, char filler)
        {
            var builder = new StringBuilder(source.Length);
            builder.Append(source.AsSpan(0, visibleLength));

            var fillerCount = source.Length - visibleLength;
            for (var i = 0; i < fillerCount; i++)
            {
                builder.Append(filler);
            }

            return builder.ToString();
        }
    }
}
