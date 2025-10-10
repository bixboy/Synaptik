using System.Collections;
using UnityEngine;
using TMPro;

namespace Synaptik.Game
{
    [DisallowMultipleComponent]
    public class UIObjectShaker : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField, Tooltip("Le GameObject UI à faire trembler (doit avoir un RectTransform)")]
        private GameObject _targetUI;

        [Header("Shake Settings")]
        [SerializeField] private float _shakePower = 10f;
        [SerializeField] private float _shakeSpeed = 25f;
        [SerializeField] private float _shakeDuration = 1f;

        [Header("Floating Number Settings")]
        
        [SerializeField] 
        private GameObject _floatingNumberPrefab;
        
        [SerializeField] 
        private RectTransform _floatingNumberSpawn;
        
        [SerializeField] 
        private Vector2 _floatOffset = new Vector2(0f, 60f);
        
        [SerializeField] private float _floatDuration = 1f;
        
        [SerializeField] 
        private float _floatRandomAngle = 25f;
        
        [SerializeField] 
        private float _floatRandomSpeedFactor = 0.25f;

        private RectTransform _targetRect;
        private Vector3 _originalPosition;
        private float _shakeTimer;
        private bool _isShaking;

        private void Awake()
        {
            if (_targetUI)
                _targetRect = _targetUI.GetComponent<RectTransform>();

            if (_targetRect)
                _originalPosition = _targetRect.anchoredPosition;
        }

        private void Start()
        {
            if (MistrustManager.Instance)
                MistrustManager.Instance.OnMistrust += HandleMistrust;
        }

        private void OnDestroy()
        {
            if (MistrustManager.Instance)
                MistrustManager.Instance.OnMistrust -= HandleMistrust;
        }

        private void Update()
        {
            if (!_isShaking || !_targetRect)
                return;

            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0f)
            {
                StopShake();
                return;
            }

            float offsetX = (Mathf.PerlinNoise(Time.time * _shakeSpeed, 0f) - 0.5f) * 2f * _shakePower;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * _shakeSpeed) - 0.5f) * 2f * _shakePower;

            _targetRect.anchoredPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);
        }

        private void HandleMistrust(float value)
        {
            Shake(_shakeDuration);
            SpawnFloatingNumber(value);
        }

        public void Shake(float duration)
        {
            if (!_targetRect)
            {
                Debug.LogWarning("[UIObjectShaker] Aucun RectTransform assigné !");
                return;
            }

            _shakeTimer = duration;
            _isShaking = true;
        }

        public void StopShake()
        {
            _isShaking = false;
            _shakeTimer = 0f;

            if (_targetRect)
                _targetRect.anchoredPosition = _originalPosition;
        }

        public void RecalibrateOrigin()
        {
            if (_targetRect)
                _originalPosition = _targetRect.anchoredPosition;
        }

        private void SpawnFloatingNumber(float value)
        {
            if (!_floatingNumberPrefab || !_floatingNumberSpawn)
            {
                Debug.LogWarning("[UIObjectShaker] Pas de prefab ou de point de spawn assigné !");
                return;
            }

            GameObject instance = Instantiate(_floatingNumberPrefab, _floatingNumberSpawn);
            RectTransform rect = instance.GetComponentInChildren<RectTransform>();
            TextMeshProUGUI text = instance.GetComponentInChildren<TextMeshProUGUI>();

            if (!rect || !text)
            {
                Debug.LogWarning("[UIObjectShaker] Le prefab du nombre flottant doit contenir un TextMeshProUGUI !");
                Destroy(instance);
                return;
            }

            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            instance.SetActive(true);

            if (value > 0f)
            {
                text.text = "+";
                text.color = Color.green;
            }
            else if (value < 0f)
            {
                text.text = "−";
                text.color = Color.red;
            }
            else
            {
                text.text = "";
                text.color = Color.white;
            }

            text.alpha = 1f;

            float randomAngle = Random.Range(-_floatRandomAngle, _floatRandomAngle);
            Vector2 randomDir = Quaternion.Euler(0f, 0f, randomAngle) * _floatOffset;
            float randomSpeedFactor = Random.Range(1f - _floatRandomSpeedFactor, 1f + _floatRandomSpeedFactor);
            float finalDuration = Mathf.Max(0.1f, _floatDuration * randomSpeedFactor);

            StartCoroutine(AnimateFloatingNumber(rect, text, randomDir, finalDuration));
        }

        private IEnumerator AnimateFloatingNumber(RectTransform rect, TextMeshProUGUI text, Vector2 offset, float duration)
        {
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            Vector2 targetPos = startPos + offset;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                text.alpha = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}
