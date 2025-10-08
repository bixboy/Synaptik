using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Synaptik.Game
{
    public class DialogueBubble : MonoBehaviour
    {
        [SerializeField] private GameObject _bubbleGameObject;
        [SerializeField] private TextMeshProUGUI _label;
        private float _remainingTime;
        [SerializeField] private bool _lookAtCamera = true;
        [SerializeField, ShowIf("_lookAtCamera")] private Camera _camera;

        private void Awake()
        {
            
            _camera = Camera.main;
            Hide();
        }

        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            if (_bubbleGameObject != null && _bubbleGameObject.activeSelf)
            {
                if (_remainingTime <= 0f)
                {
                    Hide();
                }
                else
                {
                    _remainingTime -= Time.deltaTime;
                    if (_lookAtCamera && _camera != null)
                    {
                        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
                            _camera.transform.rotation * Vector3.up);
                    }
                }
            }
            
        }

        public void ShowFor(string emojiLine, float duration)
        {
            if (string.IsNullOrEmpty(emojiLine) || duration <= 0f)
            {
                return;
            }
            if (_label != null)
            {
                _label.text = emojiLine;
            }
            _bubbleGameObject?.SetActive(true);
            _remainingTime = duration;
            
        }

        private void Hide()
        {
            _bubbleGameObject?.SetActive(false);
            _remainingTime = 0f;
        }
    }
}