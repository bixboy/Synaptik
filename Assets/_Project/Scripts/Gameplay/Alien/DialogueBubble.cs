using TMPro;
using UnityEngine;

namespace Synaptik.Game
{
    public class DialogueBubble : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, 0f);

        private static DialogueBubble _instance;
        private Alien _target;
        private float _remainingTime;
        private Camera _camera;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (_root == null)
            {
                _root = GetComponent<RectTransform>();
            }
            _camera = Camera.main;
            Hide();
        }

        private void Update()
        {
            if (_target == null || _remainingTime <= 0f)
            {
                if (_root.gameObject.activeSelf)
                {
                    Hide();
                }

                return;
            }

            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0f)
            {
                Hide();
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            var worldPos = _target.transform.position + _offset;
            var screenPos = _camera.WorldToScreenPoint(worldPos);
            _root.position = screenPos;
        }

        public static void ShowFor(Alien alien, string emojiLine, float duration)
        {
            if (alien == null || string.IsNullOrEmpty(emojiLine))
            {
                return;
            }

            if (_instance == null)
            {
                return;
            }

            _instance.Show(alien, emojiLine, duration);
        }

        private void Show(Alien alien, string emojiLine, float duration)
        {
            _target = alien;
            _remainingTime = duration;
            if (_label != null)
            {
                _label.text = emojiLine;
            }

            if (_root != null && !_root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(true);
            }
        }

        private void Hide()
        {
            _target = null;
            _remainingTime = 0f;
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
            }
        }
    }
}