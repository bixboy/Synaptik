using System;
using UnityEngine;

namespace TextTools
{
    public static class TextFeedBack
    {
        public static string ProgressiveDisplayLerp(string a_text, float a_lerp)
        {
            int textLen = a_text.Length;
            int textPos = (int)Mathf.Clamp(textLen * a_lerp, 0, textLen);

            string result =  a_text.Substring(0, textPos);
            return result;
        }
        public static string ProgressiveDisplayLerp(string a_text, float a_lerp, char a_filler)
        {
            int textLen = a_text.Length;
            int textPos = (int)Mathf.Clamp(textLen * a_lerp, 0, textLen);

            string result =  a_text.Substring(0, textPos);
            for (int i = 0; i < textLen - textPos; i++)
            {
                result += a_filler;
            }
            return result;
        }
        
        public static string ProgressiveDisplayTimeSpacing(string a_text, float a_spacingTime, float a_time)
        {
            int textLen = a_text.Length;
            int textPos = (int)Mathf.Clamp(a_time / a_spacingTime, 0, textLen);

            string result =  a_text.Substring(0, textPos);
            return result;
        }
        
        public static string ProgressiveDisplayTimeSpacing(string a_text, float a_spacingTime, float a_time, char a_filler)
        {
            int textLen = a_text.Length;
            int textPos = (int)Mathf.Clamp(a_time / a_spacingTime, 0, textLen);

            string result =  a_text.Substring(0, textPos);
            for (int i = 0; i < textLen - textPos; i++)
            {
                result += a_filler;
            }
            return result;
        }
    }
}
