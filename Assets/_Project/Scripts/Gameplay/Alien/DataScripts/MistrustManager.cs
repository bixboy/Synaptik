using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class MistrustManager : MonoBehaviour
{
    public static MistrustManager Instance { get; private set; }

    [SerializeField]
    private Slider mistrustSlider;

    [SerializeField]
    private Vector2 mistrustRange = new(0f, 100f);

    [SerializeField]
    private int initialMistrust = 50;

    private int mistrustValue;
    private bool minThresholdTriggered;
    private bool maxThresholdTriggered;

    public delegate void MistrustDelegate(float valueDelta);
    public event MistrustDelegate OnMistrust;

    public event Action OnMistrustMinReached;
    public event Action OnMistrustMaxReached;

    public int CurrentMistrust => mistrustValue;
    public int MinMistrust => Mathf.RoundToInt(mistrustRange.x);
    public int MaxMistrust => Mathf.RoundToInt(mistrustRange.y);

    private void Awake()
    {
        mistrustValue = Mathf.Clamp(initialMistrust, MinMistrust, MaxMistrust);
        if (mistrustSlider != null)
        {
            mistrustSlider.minValue = MinMistrust;
            mistrustSlider.maxValue = MaxMistrust;
            mistrustSlider.value = mistrustValue;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EvaluateThresholds();
    }

    public void AddMistrust(int amount)
    {
        UpdateMistrust(amount);
    }

    public void RemoveMistrust(int amount)
    {
        UpdateMistrust(-amount);
    }

    public void ResetMistrust()
    {
        UpdateMistrust(initialMistrust - mistrustValue);
    }

    private void UpdateMistrust(int delta)
    {
        var clamped = Mathf.Clamp(mistrustValue + delta, MinMistrust, MaxMistrust);
        var appliedDelta = clamped - mistrustValue;
        mistrustValue = clamped;

        if (mistrustSlider != null)
        {
            mistrustSlider.value = mistrustValue;
        }

        if (appliedDelta != 0)
        {
            OnMistrust?.Invoke(appliedDelta);
        }

        EvaluateThresholds();
    }

    private void OnValidate()
    {
        initialMistrust = Mathf.Clamp(initialMistrust, (int)mistrustRange.x, (int)mistrustRange.y);
    }

    private void EvaluateThresholds()
    {
        if (mistrustValue <= MinMistrust)
        {
            if (!minThresholdTriggered)
            {
                minThresholdTriggered = true;
                OnMistrustMinReached?.Invoke();
            }
        }
        else
        {
            minThresholdTriggered = false;
        }

        if (mistrustValue >= MaxMistrust)
        {
            if (!maxThresholdTriggered)
            {
                maxThresholdTriggered = true;
                OnMistrustMaxReached?.Invoke();
            }
        }
        else
        {
            maxThresholdTriggered = false;
        }
    }
}
