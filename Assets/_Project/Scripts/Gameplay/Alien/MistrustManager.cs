using UnityEngine;
using UnityEngine.UI;

public class MistrustManager : MonoBehaviour
{
    
    [SerializeField] private Slider _mistrustSlider;
    [SerializeField] private Vector2 _mistrustRange = new Vector2(0, 100);
    [SerializeField] private int _initialMistrust = 50;
    
    private int _mistrustValue;
    
    public static MistrustManager Instance;
    
    public delegate void MistrustDelegate(float value);
    public event MistrustDelegate OnMistrust;
    
    private void Awake()
    {
        _mistrustValue = Mathf.Clamp(_initialMistrust, (int)_mistrustRange.x, (int)_mistrustRange.y);
        if (_mistrustSlider != null)
        {
            _mistrustSlider.minValue = _mistrustRange.x;
            _mistrustSlider.maxValue = _mistrustRange.y;
            _mistrustSlider.value = _mistrustValue;
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    public void AddMistrust(int amount)
    {
        _mistrustValue = Mathf.Clamp(_mistrustValue + amount, (int)_mistrustRange.x, (int)_mistrustRange.y);
        
        if (_mistrustSlider)
        {
            _mistrustSlider.value = _mistrustValue;
            OnMistrust?.Invoke(amount);
        }
    }
    
    public void RemoveMistrust(int amount)
    {
        _mistrustValue = Mathf.Clamp(_mistrustValue - amount, (int)_mistrustRange.x, (int)_mistrustRange.y);
        
        if (_mistrustSlider)
        {
            _mistrustSlider.value = _mistrustValue;
            OnMistrust?.Invoke(-amount);
        }
    }

    private void OnValidate()
    {
        _initialMistrust = Mathf.Clamp(_initialMistrust, (int)_mistrustRange.x, (int)_mistrustRange.y);
    }
}
