using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class MistrustGameOutcomeHandler : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onWin;

    [SerializeField]
    private UnityEvent onGameOver;

    [SerializeField]
    private bool triggerOnlyOnce = true;

    [SerializeField]
    private bool pauseTimeOnOutcome = true;

    [SerializeField]
    private EndGameUI endGameUI;
    
    [SerializeField]
    private GameObject noteBook;

    private bool hasTriggeredOutcome;
    private bool isSubscribed;
    private float cachedTimeScale = 1f;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    public void ResetOutcomeState()
    {
        hasTriggeredOutcome = false;
        RestoreTimeScaleIfNeeded();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (!MistrustManager.Instance)
            return;

        MistrustManager.Instance.OnMistrustMinReached += HandleGameOver;
        GameManager.Instance.OnAllTaskEnd += HandleWin;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
            return;

        if (MistrustManager.Instance != null)
        {
            MistrustManager.Instance.OnMistrustMinReached -= HandleGameOver;
            GameManager.Instance.OnAllTaskEnd -= HandleWin;
        }

        isSubscribed = false;
    }

    private void OnDisable()
    {
        RestoreTimeScaleIfNeeded();
        Unsubscribe();
    }

    private void HandleWin()
    {
        TriggerOutcome(true);
    }

    private void HandleGameOver()
    {
        TriggerOutcome(false);
    }

    private void TriggerOutcome(bool win)
    {
        if (triggerOnlyOnce && hasTriggeredOutcome)
            return;

        hasTriggeredOutcome = true;

        if (pauseTimeOnOutcome)
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (win)
        {
            Win();   
        }
        else
        {
            GameOver();   
        }

    }

    private void RestoreTimeScaleIfNeeded()
    {
        if (!pauseTimeOnOutcome)
            return;

        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = cachedTimeScale;
        }
    }

    public void Win()
    {
        noteBook.gameObject.SetActive(false);
        endGameUI.gameObject.SetActive(true);
        
        endGameUI.ShowWinSequence();
        
    }

    public void GameOver()
    {
        noteBook.gameObject.SetActive(false);
        endGameUI.gameObject.SetActive(true);
        
        endGameUI.ShowLoseSequence();
        
    }
}
