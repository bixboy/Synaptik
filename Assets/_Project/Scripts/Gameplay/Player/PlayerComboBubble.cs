using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synaptik.Game
{
    [DisallowMultipleComponent]
    public class PlayerComboBubble : MonoBehaviour
    {
        [Header("Bubble Layout")]
        [SerializeField] private float _verticalOffset = 2.6f;
        [SerializeField] private Vector2 _minBubbleSize = new Vector2(120f, 60f);
        [SerializeField] private Vector2 _padding = new Vector2(16f, 10f);
        [SerializeField] private float _worldScale = 0.03f;
        [SerializeField] private Color _backgroundColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private TMP_FontAsset _fontAsset;
        [SerializeField] private float _defaultLifetime = 1.75f;

        private GameObject _bubbleRoot;
        private RectTransform _bubbleRect;
        private RectTransform _panelRect;
        private TextMeshProUGUI _label;
        private float _remainingTime;
        private Camera _camera;

        private const float PanelPadding = 6f;

        private void Awake()
        {
            EnsureHierarchy();
            HideImmediate();
        }

        private void OnEnable()
        {
            if (_bubbleRoot)
                _bubbleRoot.SetActive(false);
        }

        private void OnDisable()
        {
            HideImmediate();
        }

        private void LateUpdate()
        {
            if (_remainingTime <= 0f)
                return;

            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0f)
            {
                HideImmediate();
                return;
            }

            UpdateLookAt();
        }

        public void Show(string text, float duration)
        {
            EnsureHierarchy();

            if (!_label)
                return;

            _label.text = text ?? string.Empty;
            AdjustBubbleSize();

            if (_bubbleRoot && !_bubbleRoot.activeSelf)
                _bubbleRoot.SetActive(true);

            _remainingTime = duration > 0f ? duration : _defaultLifetime;
            UpdateLookAt();
        }

        public void HideImmediate()
        {
            _remainingTime = 0f;
            if (_bubbleRoot)
                _bubbleRoot.SetActive(false);
        }

        private void EnsureHierarchy()
        {
            if (_bubbleRoot && _label)
                return;

            _camera = Camera.main;

            _bubbleRoot = new GameObject("PlayerComboBubble", typeof(RectTransform));
            _bubbleRoot.transform.SetParent(transform, false);
            _bubbleRect = (RectTransform)_bubbleRoot.transform;
            _bubbleRect.localPosition = new Vector3(0f, _verticalOffset, 0f);
            _bubbleRect.localScale = Vector3.one * Mathf.Max(0.0001f, _worldScale);
            _bubbleRect.pivot = new Vector2(0.5f, 0f);

            var canvas = _bubbleRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = _camera;

            var scaler = _bubbleRoot.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(_bubbleRect, false);

            _panelRect = (RectTransform)panel.transform;
            _panelRect.anchorMin = Vector2.zero;
            _panelRect.anchorMax = Vector2.one;
            _panelRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
            _panelRect.offsetMax = new Vector2(-PanelPadding, -PanelPadding);

            var panelImage = panel.GetComponent<Image>();
            panelImage.color = _backgroundColor;
            panelImage.raycastTarget = false;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            textObject.transform.SetParent(panel.transform, false);

            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(_padding.x, _padding.y);
            textRect.offsetMax = new Vector2(-_padding.x, -_padding.y);

            _label = textObject.AddComponent<TextMeshProUGUI>();
            _label.font = _fontAsset ? _fontAsset : TMP_Settings.defaultFontAsset;
            _label.color = _textColor;
            _label.alignment = TextAlignmentOptions.Center;
            _label.textWrappingMode = TextWrappingModes.Normal;
            _label.raycastTarget = false;
            _label.text = string.Empty;
        }
        
        private void AdjustBubbleSize()
        {
            if (!_label || !_bubbleRect)
                return;

            _label.ForceMeshUpdate();
            Vector2 textSize = _label.GetPreferredValues(_label.text);

            Vector2 finalSize = new Vector2(
                Mathf.Max(_minBubbleSize.x, textSize.x + _padding.x * 2f),
                Mathf.Max(_minBubbleSize.y, textSize.y + _padding.y * 2f)
            );

            _bubbleRect.sizeDelta = finalSize;
        }

        private void UpdateLookAt()
        {
            if (!_bubbleRect)
                return;

            if (!_camera)
                _camera = Camera.main;

            if (!_camera)
                return;

            var forward = _camera.transform.rotation * Vector3.forward;
            var up = _camera.transform.rotation * Vector3.up;
            _bubbleRect.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}
