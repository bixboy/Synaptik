using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerComboBubble : MonoBehaviour
{
    [Header("Bubble Setup")]
    [SerializeField]
    private GameObject bubblePrefab;

    [SerializeField]
    private float verticalOffset = 2.6f;

    [SerializeField]
    private float worldScale = 0.03f;

    [Header("Layout Settings")]
    [SerializeField]
    private Vector2 minBubbleSize = new(120f, 60f);

    [SerializeField]
    private Vector2 padding = new(16f, 10f);

    [SerializeField]
    private float defaultLifetime = 1.75f;

    [Header("Visual Settings")]
    [SerializeField]
    private Color backgroundColor = new(1f, 1f, 1f, 0.4f);

    [SerializeField]
    private Color textColor = Color.black;

    [SerializeField]
    private TMP_FontAsset fontAsset;

    private GameObject bubbleInstance;
    private RectTransform bubbleRect;
    private Image backgroundImage;
    private TextMeshProUGUI label;
    private float remainingTime;
    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = Camera.main;
        HideImmediate();
    }

    private void OnDisable()
    {
        HideImmediate();
    }

    private void LateUpdate()
    {
        if (remainingTime <= 0f)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            HideImmediate();
            return;
        }

        UpdateLookAt();
    }

    public void Show(string text, float duration)
    {
        EnsureInstance();

        if (label == null)
        {
            return;
        }

        label.text = text ?? string.Empty;
        AdjustBubbleSize();

        if (bubbleInstance != null && !bubbleInstance.activeSelf)
        {
            bubbleInstance.SetActive(true);
        }

        remainingTime = duration > 0f ? duration : defaultLifetime;
        UpdateLookAt();
    }

    public void HideImmediate()
    {
        remainingTime = 0f;
        if (bubbleInstance != null)
        {
            bubbleInstance.SetActive(false);
        }
    }

    private void EnsureInstance()
    {
        if (bubbleInstance != null)
        {
            return;
        }

        if (bubblePrefab == null)
        {
            Debug.LogError("[PlayerComboBubble] No bubble prefab assigned!");
            return;
        }

        targetCamera = Camera.main;

        bubbleInstance = Instantiate(bubblePrefab, transform);
        bubbleRect = bubbleInstance.GetComponent<RectTransform>();
        bubbleRect.localPosition = new Vector3(0f, verticalOffset, 0f);
        bubbleRect.localScale = Vector3.one * Mathf.Max(0.0001f, worldScale);
        bubbleRect.pivot = new Vector2(0.5f, 0f);

        label = bubbleInstance.GetComponentInChildren<TextMeshProUGUI>(true);
        backgroundImage = bubbleInstance.GetComponentInChildren<Image>(true);

        if (label != null)
        {
            label.color = textColor;
            label.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        bubbleInstance.SetActive(false);
    }

    private void AdjustBubbleSize()
    {
        if (label == null || bubbleRect == null)
        {
            return;
        }

        label.ForceMeshUpdate();
        var textSize = label.GetPreferredValues(label.text);

        var finalSize = new Vector2(
            Mathf.Max(minBubbleSize.x, textSize.x + padding.x * 2f),
            Mathf.Max(minBubbleSize.y, textSize.y + padding.y * 2f)
        );

        bubbleRect.sizeDelta = finalSize;
    }

    private void UpdateLookAt()
    {
        if (bubbleRect == null)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        var forward = targetCamera.transform.rotation * Vector3.forward;
        var up = targetCamera.transform.rotation * Vector3.up;
        bubbleRect.rotation = Quaternion.LookRotation(forward, up);
    }
}
