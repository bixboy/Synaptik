using System;
using System.Collections;
using UnityEngine;

namespace Synaptik.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerComboBubble))]
    public class PlayerInteractionFeedback : MonoBehaviour
    {
        [Header("Durées d'affichage")]
        [SerializeField, Min(0.2f)] private float _successDuration = 2.2f;
        [SerializeField, Min(0.2f)] private float _warningDuration = 2.6f;
        [SerializeField, Min(0.2f)] private float _infoDuration = 2.4f;

        [Header("Couleurs de bulles")]
        [SerializeField] private Color _successBackground = new Color(0.15f, 0.65f, 0.35f, 0.75f);
        [SerializeField] private Color _warningBackground = new Color(0.75f, 0.25f, 0.2f, 0.8f);
        [SerializeField] private Color _infoBackground = new Color(0.15f, 0.35f, 0.75f, 0.75f);
        [SerializeField] private Color _successText = Color.white;
        [SerializeField] private Color _warningText = Color.white;
        [SerializeField] private Color _infoText = Color.white;

        [Header("Audio optionnel")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _successClip;
        [SerializeField] private AudioClip _warningClip;
        [SerializeField] private AudioClip _infoClip;

        [Header("Anti-spam")]
        [SerializeField, Min(0f)] private float _repeatCooldown = 0.75f;

        private PlayerComboBubble _comboBubble;
        private Coroutine _waitForGameManagerCoroutine;
        private string _lastMessage;
        private float _lastMessageTime;

        private void Awake()
        {
            _comboBubble = GetComponent<PlayerComboBubble>();
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            TrySubscribeToGameManager();
        }

        private void Start()
        {
            if (_comboBubble == null)
            {
                _comboBubble = GetComponent<PlayerComboBubble>();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromGameManager();
        }

        private void TrySubscribeToGameManager()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsInitialized)
            {
                GameManager.Instance.OnTaskEnd += HandleMissionCompleted;
                GameManager.Instance.OnMissionRegistered += HandleMissionRegistered;
            }
            else if (_waitForGameManagerCoroutine == null)
            {
                _waitForGameManagerCoroutine = StartCoroutine(WaitForGameManager());
            }
        }

        private void UnsubscribeFromGameManager()
        {
            if (_waitForGameManagerCoroutine != null)
            {
                StopCoroutine(_waitForGameManagerCoroutine);
                _waitForGameManagerCoroutine = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTaskEnd -= HandleMissionCompleted;
                GameManager.Instance.OnMissionRegistered -= HandleMissionRegistered;
            }
        }

        private IEnumerator WaitForGameManager()
        {
            while (GameManager.Instance == null || !GameManager.Instance.IsInitialized)
            {
                yield return null;
            }

            GameManager.Instance.OnTaskEnd += HandleMissionCompleted;
            GameManager.Instance.OnMissionRegistered += HandleMissionRegistered;
            _waitForGameManagerCoroutine = null;
        }

        private void HandleMissionRegistered(Mission mission)
        {
            if (string.IsNullOrWhiteSpace(mission.Title))
            {
                return;
            }

            ShowInfo($"Nouvelle mission : {mission.Title}");
        }

        private void HandleMissionCompleted(Mission mission)
        {
            if (string.IsNullOrWhiteSpace(mission.Title))
            {
                return;
            }

            ShowSuccess($"Mission terminée : {mission.Title}");
        }

        public void ShowPickupSuccess(string itemName)
        {
            string message = string.IsNullOrWhiteSpace(itemName) ? "Objet ramassé !" : $"Objet ramassé : {itemName}";
            Show(message, _successDuration, _successBackground, _successText, _successClip);
        }

        public void ShowPickupSwap(string previousItemName, string newItemName)
        {
            if (string.IsNullOrWhiteSpace(previousItemName))
            {
                ShowPickupSuccess(newItemName);
                return;
            }

            string message = string.IsNullOrWhiteSpace(newItemName)
                ? $"Tu as reposé {previousItemName}."
                : $"{previousItemName} → {newItemName}";

            Show(message, _infoDuration, _infoBackground, _infoText, _infoClip);
        }

        public void ShowPickupUnavailable()
        {
            Show("Rapproche-toi d'un objet à ramasser.", _warningDuration, _warningBackground, _warningText, _warningClip);
        }

        public void ShowNoItemToDrop()
        {
            Show("Tu n'as rien en main.", _warningDuration, _warningBackground, _warningText, _warningClip);
        }

        public void ShowDropOnGround(string itemName)
        {
            string message = string.IsNullOrWhiteSpace(itemName) ? "Objet posé au sol." : $"{itemName} est posé au sol.";
            Show(message, _infoDuration, _infoBackground, _infoText, _infoClip);
        }

        public void ShowGiftAccepted(string itemName, string targetName)
        {
            string cleanTarget = string.IsNullOrWhiteSpace(targetName) ? "l'alien" : targetName;
            string message = string.IsNullOrWhiteSpace(itemName)
                ? $"{cleanTarget} apprécie ton cadeau !"
                : $"{cleanTarget} accepte {itemName} !";
            Show(message, _successDuration, _successBackground, _successText, _successClip);
        }

        public void ShowGiftRefused(string itemName, string targetName)
        {
            string cleanTarget = string.IsNullOrWhiteSpace(targetName) ? "l'alien" : targetName;
            string itemSection = string.IsNullOrWhiteSpace(itemName) ? "ce cadeau" : itemName;
            Show($"{cleanTarget} refuse {itemSection}.", _warningDuration, _warningBackground, _warningText, _warningClip);
        }

        public void ShowNeedToApproach(string targetName)
        {
            string cleanTarget = string.IsNullOrWhiteSpace(targetName) ? "l'alien" : targetName;
            Show($"Approche-toi encore de {cleanTarget} pour lui donner l'objet.", _infoDuration, _infoBackground, _infoText, _infoClip);
        }

        public void ShowItemConsumed(string itemName)
        {
            string message = string.IsNullOrWhiteSpace(itemName) ? "L'objet a été utilisé." : $"{itemName} a été utilisé.";
            Show(message, _successDuration, _successBackground, _successText, _successClip);
        }

        public void ShowFriendlyWithoutItem()
        {
            Show("Tu dois tenir un objet pour offrir quelque chose.", _warningDuration, _warningBackground, _warningText, _warningClip);
        }

        public void ShowComboNoTarget(Emotion emotion, Behavior behavior, bool isHoldingItem)
        {
            string message;

            if (behavior == Behavior.Talking)
            {
                message = "Personne à proximité à qui parler.";
            }
            else if (behavior == Behavior.Action && emotion == Emotion.Friendly && !isHoldingItem)
            {
                message = "Prends un objet avant d'être gentil.";
            }
            else
            {
                message = "Rien ne réagit à cette action.";
            }

            Show(message, _warningDuration, _warningBackground, _warningText, _warningClip);
        }

        public void ShowInfo(string message)
        {
            Show(message, _infoDuration, _infoBackground, _infoText, _infoClip);
        }

        public void ShowSuccess(string message)
        {
            Show(message, _successDuration, _successBackground, _successText, _successClip);
        }

        private void Show(string message, float duration, Color background, Color textColor, AudioClip clip)
        {
            if (_comboBubble == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (_repeatCooldown > 0f && string.Equals(message, _lastMessage, StringComparison.Ordinal))
            {
                if (Time.time - _lastMessageTime < _repeatCooldown)
                {
                    return;
                }
            }

            _comboBubble.Show(message, duration, background, textColor);
            _lastMessage = message;
            _lastMessageTime = Time.time;

            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}
