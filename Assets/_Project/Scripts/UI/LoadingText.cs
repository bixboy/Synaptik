using System;
using System.Collections;
using System.Collections.Generic;
using TextTools;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public TextMeshProUGUI LoaddingText;

    private float currentTime;
    private string currentLine;
    private string fullText;

    private bool isDisplaying;

    [SerializeField] private float delayBetweenLines = 0.2f;

    private void Start()
    {
        Loading(0);
    }

    public void StartText(List<string> lines)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayLinesRoutine(lines));
    }

    private IEnumerator DisplayLinesRoutine(List<string> lines)
    {
        fullText = "";
        currentLine = "";
        Text.text = "";
        isDisplaying = false;
        
        foreach (string line in lines)
        {
            AddNewLine(line);
            
            yield return new WaitUntil(() => isDisplaying == false);

            yield return new WaitForSeconds(delayBetweenLines);
        }
    }

    public void AddNewLine(string text)
    {
        if (isDisplaying)
            fullText += currentLine + "\n";

        currentLine = text;
        currentTime = 0f;
        isDisplaying = true;
    }

    public void Loading(float lerp)
    {
        LoaddingText.text = $"[{TextFeedBack.ProgressiveDisplayLerp("000000000000000000000000", lerp, '-')}]";
    }

    private void Update()
    {
        if (!isDisplaying) return;

        string progressive = TextFeedBack.ProgressiveDisplayTimeSpacing(currentLine, 0.01f, currentTime);
        Text.text = fullText + progressive;

        currentTime += Time.deltaTime;

        if (progressive.Length >= currentLine.Length)
        {
            fullText += currentLine + "\n";
            currentLine = "";
            isDisplaying = false;
        }
    }
}
