using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private Transform _aimZone;
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
            Transform origin = _aimZone != null ? _aimZone : transform;
            Alien alien = TargetingUtil.FindAlienInFront(origin, _interactRadius, _interactHalfFov, _alienMask);
            if (alien == null)
            {
                if (!_held && emotion == Emotion.Curious && behavior == Behavior.Action)
                    PickUp();
                else if (_held && emotion == Emotion.Friendly && behavior == Behavior.Action)
                    DropItem();
                
                return;
            }

            if (behavior == Behavior.Action)
            {
                switch (emotion)
                {
                    case Emotion.Anger: // Hit
                        break;
                    case Emotion.Curious: // Ramasser
                    {
                        PickUp();
                        break;
                    }
                        
                    case Emotion.Fearful: // Courir
                        break;
                    case Emotion.Friendly: // Donne
                    {
                        if (alien.Definition.Reactions.TryFindItemRule(_heldItemId, out var itemRule))
                        {
                            alien.TryReceiveItem(_heldItemId);
                            DropItem(true);
                            Debug.Log($"Give item {itemRule.ExpectedItemId} to alien {alien.Definition.name}");
                            return;
                        }
                        DropItem();
                        
                        Debug.Log("Drop item in front of alien");
                        return;
                    }
                        
                }
            }
            else if (behavior == Behavior.Talking)
            {
                switch (emotion)
                {
                    case Emotion.Anger: // Insulter
                        break;
                    case Emotion.Curious: // Curieux
                        break;
                    case Emotion.Fearful: // Crie
                        break;
                    case Emotion.Friendly: // Complimenter 
                        break;
                }
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
                if (!holdable) 
                    continue;


                if (_held && holdable == _held)
                    continue;

                float d = Vector3.SqrMagnitude(h.transform.position - origin);
                if (d < bestDist) { bestDist = d; best = holdable; }
            }
            
            if (!best)
            {
                if (!_held)
                    Debug.Log("PickUp: aucun objet à portée");
                
                return;
            }
            
            if (_held)
            {
                Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                _held.Drop(v);
                _held = null;
            }
            
            best.Pick(_handSocket ? _handSocket : transform);
            _held = best;
        }

        public void DropItem(bool destroyItem = false)
        {
            if (!_held)
                return;
            
            if (destroyItem)
            {
                Destroy(_held.gameObject);
                _held = null;
                
                return;
            }
            Transform origin = _aimZone ? _aimZone : transform;
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
                    if (alien.TryReceiveItem(_heldItemId));
                    else
                    {
                        Vector3 v = _dropForwardSpeed > 0f ? transform.forward * _dropForwardSpeed : Vector3.zero;
                        _held.Drop(v);
                    }
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