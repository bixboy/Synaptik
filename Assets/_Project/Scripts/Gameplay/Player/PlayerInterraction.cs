using System;
using UnityEngine;

namespace Synaptik.Game
{
    public class PlayerInteraction : MonoBehaviour
    {
        
        [Header("Pickup/Drop Settings")]
        [SerializeField] private Transform _handSocket;         // vide placé dans la main
        [SerializeField] private float _pickupRadius = 1.2f;    // portée
        [SerializeField] private LayerMask _pickupMask = ~0;    // couche(s) des objets
        [SerializeField] private float _dropForwardSpeed = 0f;  // 0 = lâcher sans lancer

        private HoldableItem _held;
        private string _heldItemId => _held != null ? _held.ItemId : null;
        
        
        [Header("Interaction Settings")]
        [SerializeField] private Transform _aim;
        [SerializeField] private float _interactRadius = 2f;
        [SerializeField] private float _interactHalfFov = 45f;
        [SerializeField] private LayerMask _alienMask;

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
            Transform origin = _aim != null ? _aim : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _alienMask);
            if (alien == null)
            {
                PickUp();
                return;
            }
            
            alien.OnPlayerCombo(emotion, behavior);
        }

        public void PickUp()
        {
            Debug.Log("PickUp: tenter de ramasser un objet");
            Vector3 origin = _handSocket ? _handSocket.position : transform.position;
            Collider[] hits = Physics.OverlapSphere(origin, _pickupRadius, _pickupMask, QueryTriggerInteraction.Ignore);

            HoldableItem best = null;
            float bestDist = float.MaxValue;

            foreach (var h in hits)
            {
                var holdable = h.GetComponentInParent<HoldableItem>();
                if (holdable == null) continue;


                if (_held != null && holdable == _held) continue;

                float d = Vector3.SqrMagnitude(h.transform.position - origin);
                if (d < bestDist) { bestDist = d; best = holdable; }
            }
            
            if (best == null)
            {
                if (_held == null)
                    Debug.Log("PickUp: aucun objet à portée");
                return;
            }
            
            if (_held != null)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
            }
            
            best.Pick(_handSocket != null ? _handSocket : transform);
            _held = best;
        }

        public void DropItem()
        {
            if (_held == null) return;
            
            Transform origin = _aim != null ? _aim : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _alienMask);

            if (!alien)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
            }
            else
            {
                if (alien.IsWithinReceiveRadius(origin.position))
                {
                    alien.TryReceiveItem(_heldItemId);
                }
            }
            _held = null;
        }

        public void OnDrawGizmos()
        {
            if (_handSocket != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_handSocket.position, _pickupRadius);
            }

            if (_aim != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_aim.position, _interactRadius);
                Gizmos.DrawLine(_aim.position, _aim.position + Quaternion.Euler(0f, _interactHalfFov, 0f) * _aim.forward * _interactRadius);
                Gizmos.DrawLine(_aim.position, _aim.position + Quaternion.Euler(0f, -_interactHalfFov, 0f) * _aim.forward * _interactRadius);
            }
        }

        public void Interact(Emotion e, Behavior b)
        {
            if (b == Behavior.Talking)
            {
                switch (e)
                {
                    case Emotion.Curious:
                        Debug.Log("Interagir: poser une question");
                        break;
                    case Emotion.Friendly:
                        Debug.Log("Interagir: faire un compliment");
                        break;
                    case Emotion.Fearful:
                        Debug.Log("Interagir: montrer qu'on cède");
                        break;
                    case Emotion.Anger:
                        Debug.Log("Interagir: insulter");
                        break;
                    default:
                        Debug.Log("Interagir: poser une question");
                        break;    
                }
            }
            else if (b == Behavior.Action)
            {
                switch (e)
                {
                    case Emotion.Curious:
                        PickUp();
                        break;
                    case Emotion.Friendly:
                        DropItem();
                        break;
                    case Emotion.Fearful:
                        Debug.Log("Interagir: courir");
                        break;
                    case Emotion.Anger:
                        Debug.Log("Interagir: frapper");
                        break;
                    
                    default:
                        Debug.Log("Interagir: donner un objet");
                        break;    
                }
            }
        }
    }
}