using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Synaptik.Game
{
    [DisallowMultipleComponent]
    public class PlayerComboBubble : MonoBehaviour
    {
        [Header("Bubble Setup")]
        [SerializeField] private GameObject _bubblePrefab;
        [SerializeField] private float _verticalOffset = 2.6f;
        [SerializeField] private float _worldScale = 0.03f;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 _minBubbleSize = new Vector2(120f, 60f);
        [SerializeField] private Vector2 _padding = new Vector2(16f, 10f);
        [SerializeField] private float _defaultLifetime = 1.75f;

        [Header("Visual Settings")]
        [SerializeField] private Color _backgroundColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private TMP_FontAsset _fontAsset;

        private GameObject _bubbleInstance;
        private RectTransform _bubbleRect;
        private Image _backgroundImage;
        private TextMeshProUGUI _label;
        private float _remainingTime;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            HideImmediate();
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

        public void Show(string text, float duration, Color? backgroundOverride = null, Color? textColorOverride = null)
        {
            EnsureInstance();

            if (!_label)
                return;

            if (_backgroundImage)
            {
                _backgroundImage.color = backgroundOverride ?? _backgroundColor;
            }

            if (_label)
            {
                _label.color = textColorOverride ?? _textColor;
            }

            _label.text = text ?? string.Empty;
            AdjustBubbleSize();

            if (!_bubbleInstance.activeSelf)
                _bubbleInstance.SetActive(true);

            _remainingTime = duration > 0f ? duration : _defaultLifetime;
            UpdateLookAt();
        }

        public void HideImmediate()
        {
            _remainingTime = 0f;
            if (_bubbleInstance)
                _bubbleInstance.SetActive(false);
        }

        private void EnsureInstance()
        {
            if (_bubbleInstance)
                return;

            if (!_bubblePrefab)
            {
                Debug.LogError("[PlayerComboBubble] No bubble prefab assigned!");
                return;
            }

            _camera = Camera.main;

            _bubbleInstance = Instantiate(_bubblePrefab, transform);
            _bubbleRect = _bubbleInstance.GetComponent<RectTransform>();
            _bubbleRect.localPosition = new Vector3(0f, _verticalOffset, 0f);
            _bubbleRect.localScale = Vector3.one * Mathf.Max(0.0001f, _worldScale);
            _bubbleRect.pivot = new Vector2(0.5f, 0f);

            _label = _bubbleInstance.GetComponentInChildren<TextMeshProUGUI>(true);
            _backgroundImage = _bubbleInstance.GetComponentInChildren<Image>(true);

            if (_label)
            {
                _label.color = _textColor;
                if (_fontAsset)
                    _label.font = _fontAsset;
                else if (TMP_Settings.defaultFontAsset)
                    _label.font = TMP_Settings.defaultFontAsset;
            }

            if (_backgroundImage)
                _backgroundImage.color = _backgroundColor;

            _bubbleInstance.SetActive(false);
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
