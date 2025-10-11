using System.Collections;
using TMPro;
using UnityEngine;

public sealed class DialogueBubble : MonoBehaviour
{
    [Header("Références")]
    [SerializeField]
    private TextMeshProUGUI dialogueText;

    [Header("Paramètres")]
    [SerializeField]
    private float lifetime = 3f;

    [SerializeField]
    private float typingSpeed = 0.03f;

    private Transform mainCamera;

    private void Start()
    {
        var camera = Camera.main;
        if (camera != null)
        {
            mainCamera = camera.transform;
        }

        Destroy(gameObject, lifetime);
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            return;
        }

        transform.LookAt(transform.position + mainCamera.forward);
    }

    public void SetText(string text)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogueText == null)
        {
            yield break;
        }

        dialogueText.text = string.Empty;
        foreach (var letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
