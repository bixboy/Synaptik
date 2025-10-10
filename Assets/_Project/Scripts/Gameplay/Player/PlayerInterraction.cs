using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Synaptik.Game
{
    public class PlayerInteraction : MonoBehaviour, IAlienReaction
    {
        
        [Header("Pickup/Drop Settings")]
        [SerializeField] private Transform _handSocket;         // vide placé dans la main
        [SerializeField] private float _pickupRadius = 1.2f;    // portée
        [SerializeField] private LayerMask _pickupMask = ~0;    // couche(s) des objets
        [SerializeField] private float _dropForwardSpeed = 0f;  // 0 = lâcher sans lancer

        private HoldableItem _held;
        private string _heldItemId;
        
        [Header("Interaction Settings")] 
        [SerializeField] private Transform _aimZone;
        [SerializeField] private float _interactRadius = 2f;
        [SerializeField] private float _interactHalfFov = 45f;
        [SerializeField] private LayerMask _interactMask;

        private static readonly Collider[] _overlap = new Collider[64];
        private void Start()
        {
            InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
        }

        private void OnEnable()
        {
            if (InputsDetection.Instance != null)
            {
                InputsDetection.Instance.OnEmotionAction += HandleEmotionAction;
            }
        }

        private void OnDisable()
        {
            if (InputsDetection.Instance != null)
            {
                InputsDetection.Instance.OnEmotionAction -= HandleEmotionAction;
            }
        }

        private void HandleEmotionAction(Emotion emotion, Behavior behavior)
        {
            Debug.Log($"HandleEmotionAction: {emotion} + {behavior}");
            Transform origin = _aimZone != null ? _aimZone : transform;
            IInteraction interactable = TargetingUtil.FindInteractionInFront(origin, _interactRadius, _interactHalfFov, _interactMask);
            if (interactable != null) interactable?.Interact(new ActionValues(emotion, behavior), _held, this);
            else if (emotion == Emotion.Friendly && behavior == Behavior.Action && _held != null) DropItem();
        }

        
        public void PickUp()
        {
            Vector3 origin = _handSocket ? _handSocket.position : transform.position;
            int count = Physics.OverlapSphereNonAlloc(origin, _pickupRadius, _overlap, _pickupMask, QueryTriggerInteraction.Ignore);
            if (count <= 0)
            {
                if (_held == null) Debug.Log("PickUp: aucun objet à portée");
                return;
            }

            HoldableItem best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _overlap[i];
                if (!col || !col.gameObject.activeInHierarchy) continue;

                var holdable = col.GetComponentInParent<HoldableItem>();
                if (!holdable) continue;

                // ignore l'objet déjà en main ou momentanément non-prenable
                if ((_held && holdable == _held) || !holdable.CanBePicked) continue;

                float sqr = (holdable.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = holdable; }
            }

            if (!best)
            {
                if (_held == null) Debug.Log("PickUp: aucun objet à portée");
                return;
            }

            // swap si on tient déjà quelque chose
            if (_held)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
            }

            best.Pick(_handSocket ? _handSocket : transform);
            _held = best;
            _heldItemId = _held.ItemId;
        }

        public void DropItem(bool destroyItem = false)
        {
            if (!_held) return;

            if (destroyItem)
            {
                Destroy(_held.gameObject);
                _held = null;
                _heldItemId = null; // si tu utilises un ID
                return;
            }

            // tenter un don à un alien (sinon drop au sol)
            Transform origin = _aimZone ? _aimZone : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _interactMask);

            bool didGive = false;
            if (alien && alien.IsWithinReceiveRadius(origin.position))
            {
                // l'alien accepte ?
                didGive = alien.TryReceiveItem(_heldItemId);
            }

            if (didGive)
            {
                // consommé par l'alien
                Destroy(_held.gameObject);
                _held = null;
                _heldItemId = null;
            }
            else
            {
                // drop “au sol”
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
                _heldItemId = null;
            }
        }

        #region AlienReaction

        public void FeedbackAnimation(Animation a_animation)
        {
            throw new NotImplementedException();
        }
        public void FeedbackColor(Color a_color)
        {
            throw new NotImplementedException();
        }
        public void FeedbackTalking(string a_text)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        
        public void OnDrawGizmos()
        {
            if (_handSocket != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_handSocket.position, _pickupRadius);
            }

            if (_aimZone != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_aimZone.position, _interactRadius);
                Gizmos.DrawLine(_aimZone.position, _aimZone.position + Quaternion.Euler(0f, _interactHalfFov, 0f) * _aimZone.forward * _interactRadius);
                Gizmos.DrawLine(_aimZone.position, _aimZone.position + Quaternion.Euler(0f, -_interactHalfFov, 0f) * _aimZone.forward * _interactRadius);
            }
        }
    }
}