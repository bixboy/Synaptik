using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotebookEntry : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Toggle _notebookToggle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void Initialize(Mission mission)
    {
        if (_titleText != null)
            _titleText.text = mission.Title;

        if (_descriptionText != null)
            _descriptionText.text = mission.Description;

        if (_notebookToggle != null)
            _notebookToggle.isOn = mission.IsFinished;
    }
    
    public void SetToggle(bool isOn)
    {
        if (_notebookToggle != null)
            _notebookToggle.isOn = isOn;
    }
}
