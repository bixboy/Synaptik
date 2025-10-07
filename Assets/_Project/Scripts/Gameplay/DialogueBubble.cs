using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBubble : MonoBehaviour
{
    [Header("Références")]
    public TextMeshProUGUI dialogueText;

    [Header("Paramètres")]
    public float lifetime = 3f;
    public float typingSpeed = 0.03f;

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
        StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}