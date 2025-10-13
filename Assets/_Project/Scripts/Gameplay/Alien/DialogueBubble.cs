using NaughtyAttributes;
using TMPro;
using UnityEngine;

public sealed class DialogueBubble : MonoBehaviour
{
    [SerializeField]
    private GameObject bubbleGameObject;
    
    [SerializeField]
    private Transform bubbleAnchor;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private bool lookAtCamera = true;

    [SerializeField, ShowIf(nameof(lookAtCamera))]
    private Camera targetCamera;

    private float remainingTime;

    private void Awake()
    {
        targetCamera = Camera.main;
        Hide();
    }
    
    private void Update()
    {
        if (!bubbleGameObject || !bubbleGameObject.activeSelf)
            return;

        if (remainingTime <= 0f)
        {
            Hide();
            return;
        }

        remainingTime -= Time.deltaTime;

        if (!lookAtCamera)
            return;

        if (!targetCamera)
            targetCamera = Camera.main;

        if (!targetCamera)
            return;

        var forward = targetCamera.transform.rotation * Vector3.forward;
        var up = targetCamera.transform.rotation * Vector3.up;
        bubbleAnchor.transform.LookAt(bubbleAnchor.transform.position + forward, up);
    }

    public void ShowFor(string emojiLine, float duration)
    {
        if (string.IsNullOrEmpty(emojiLine) || duration <= 0f)
            return;

        if (label)
            label.text = emojiLine;

        if (bubbleGameObject)
            bubbleGameObject.SetActive(true);

        remainingTime = duration;
    }

    private void Hide()
    {
        if (bubbleGameObject)
        {
            bubbleGameObject.SetActive(false);
        }
        remainingTime = 0f;
    }
}
