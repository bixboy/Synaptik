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
        {
            return;
        }

        if (MistrustManager.Instance == null)
        {
            return;
        }

        MistrustManager.Instance.OnMistrustMinReached += HandleWin;
        MistrustManager.Instance.OnMistrustMaxReached += HandleGameOver;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
        {
            return;
        }

        if (MistrustManager.Instance != null)
        {
            MistrustManager.Instance.OnMistrustMinReached -= HandleWin;
            MistrustManager.Instance.OnMistrustMaxReached -= HandleGameOver;
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
        TriggerOutcome(onWin);
    }

    private void HandleGameOver()
    {
        TriggerOutcome(onGameOver);
    }

    private void TriggerOutcome(UnityEvent outcomeEvent)
    {
        if (triggerOnlyOnce && hasTriggeredOutcome)
        {
            return;
        }

        hasTriggeredOutcome = true;

        if (pauseTimeOnOutcome)
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        outcomeEvent?.Invoke();
    }

    private void RestoreTimeScaleIfNeeded()
    {
        if (!pauseTimeOnOutcome)
        {
            return;
        }

        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = cachedTimeScale;
        }
    }
}
