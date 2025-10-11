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

    public delegate void MistrustDelegate(float valueDelta);
    public event MistrustDelegate OnMistrust;

    private void Awake()
    {
        mistrustValue = Mathf.Clamp(initialMistrust, (int)mistrustRange.x, (int)mistrustRange.y);
        if (mistrustSlider != null)
        {
            mistrustSlider.minValue = mistrustRange.x;
            mistrustSlider.maxValue = mistrustRange.y;
            mistrustSlider.value = mistrustValue;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddMistrust(int amount)
    {
        UpdateMistrust(amount);
    }

    public void RemoveMistrust(int amount)
    {
        UpdateMistrust(-amount);
    }

    private void UpdateMistrust(int delta)
    {
        var clamped = Mathf.Clamp(mistrustValue + delta, (int)mistrustRange.x, (int)mistrustRange.y);
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
    }

    private void OnValidate()
    {
        initialMistrust = Mathf.Clamp(initialMistrust, (int)mistrustRange.x, (int)mistrustRange.y);
    }
}
