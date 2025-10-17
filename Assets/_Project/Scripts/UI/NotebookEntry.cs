using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class NotebookEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private Toggle notebookToggle;

    public void Initialize(Mission mission)
    {
        if (titleText)
        {
            titleText.text = mission.Title;
        }

        if (descriptionText)
        {
            descriptionText.text = mission.Description;
        }

        SetToggle(mission.IsFinished);
    }

    public void SetToggle(bool isOn)
    {
        if (!notebookToggle)
            return;

        notebookToggle.isOn = isOn;
    }
}
