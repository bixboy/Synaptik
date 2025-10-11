using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Synaptik.UI
{
    public sealed class LoadingText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI mainText;

        [SerializeField]
        private TextMeshProUGUI loadingText;

        [SerializeField]
        private float delayBetweenLines = 0.2f;

        [SerializeField]
        private float characterSpacing = 0.01f;

        private float currentTime;
        private string currentLine = string.Empty;
        private string fullText = string.Empty;
        private bool isDisplaying;

        private void Start()
        {
            SetLoadingProgress(0f);
        }

        public void StartText(IReadOnlyList<string> lines)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayLinesRoutine(lines));
        }

        private IEnumerator DisplayLinesRoutine(IReadOnlyList<string> lines)
        {
            fullText = string.Empty;
            currentLine = string.Empty;
            if (mainText != null)
            {
                mainText.text = string.Empty;
            }

            isDisplaying = false;

            foreach (var line in lines)
            {
                AddNewLine(line);

                yield return new WaitUntil(() => !isDisplaying);
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }

        public void AddNewLine(string text)
        {
            if (isDisplaying)
            {
                fullText += currentLine + "\n";
            }

            currentLine = text;
            currentTime = 0f;
            isDisplaying = true;
        }

        public void SetLoadingProgress(float lerp)
        {
            if (loadingText == null)
            {
                return;
            }

            loadingText.text = $"[{Core.TextFeedBack.ProgressiveDisplayLerp("000000000000000000000000", lerp, '-')}]";
        }

        private void Update()
        {
            if (!isDisplaying || mainText == null)
            {
                return;
            }

            var progressive = Core.TextFeedBack.ProgressiveDisplayTimeSpacing(currentLine, characterSpacing, currentTime);
            mainText.text = fullText + progressive;

            currentTime += Time.deltaTime;

            if (progressive.Length < currentLine.Length)
            {
                return;
            }

            fullText += currentLine + "\n";
            currentLine = string.Empty;
            isDisplaying = false;
        }
    }
}
