using UnityEngine;
using TMPro;

public class DialogueBubble : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public float lifetime = 3f;
    private Transform _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main.transform;
        Destroy(gameObject, lifetime);
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + _mainCamera.forward);
    }

    public void SetText(string text)
    {
        dialogueText.text = text;
    }
}