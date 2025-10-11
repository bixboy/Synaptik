using System;
using System.Collections;
using Synaptik.Interfaces;
using UnityEngine;

namespace Synaptik.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class HoldableItem : MonoBehaviour, IInteraction
    {
        [SerializeField]
        private string itemId;

        [Header("Respawn")]
        [SerializeField]
        private float respawnDelay = 5f;

        [SerializeField]
        private float despawnTime = 0.5f;

        [SerializeField]
        private AnimationCurve despawnAnim = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private GameObject despawnVfxPrefab;

        private Rigidbody rigidbodyComponent;
        private Collider[] colliders = Array.Empty<Collider>();
        private Transform originalParent;
        private Vector3 spawnLocation;
        private Quaternion spawnRotation;
        private Vector3 spawnScale;
        private Coroutine respawnCoroutine;
        private float currentDelay;
        private bool canTake = true;

        public bool IsHeld { get; private set; }
        public string ItemId => itemId;
        public bool CanBePicked => canTake && !IsHeld;

        private void Awake()
        {
            rigidbodyComponent = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>(true);

            spawnLocation = transform.position;
            spawnRotation = transform.rotation;
            spawnScale = transform.localScale;
        }

        public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
        {
            if (action._behavior != Behavior.Action || playerInteraction == null)
            {
                return;
            }

            switch (action._emotion)
            {
                case Emotion.Curious:
                    playerInteraction.PickUp();
                    break;
                case Emotion.Friendly when item != null:
                    playerInteraction.DropItem();
                    break;
            }
        }

        public void Pick(Transform handSocket)
        {
            if (IsHeld || !canTake)
            {
                return;
            }

            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }

            IsHeld = true;
            originalParent = transform.parent;

            rigidbodyComponent.velocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;
            rigidbodyComponent.isKinematic = true;
            rigidbodyComponent.useGravity = false;

            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            transform.SetParent(handSocket, false);
            transform.localPosition = handSocket.localPosition;
        }

        public void Drop(Vector3 inheritVelocity)
        {
            if (!IsHeld)
            {
                return;
            }

            respawnCoroutine = StartCoroutine(Respawn());

            transform.SetParent(originalParent, true);
            foreach (var collider in colliders)
            {
                collider.enabled = true;
            }

            rigidbodyComponent.isKinematic = false;
            rigidbodyComponent.useGravity = true;
            rigidbodyComponent.velocity = inheritVelocity;

            IsHeld = false;
        }

        private IEnumerator Respawn(float durationOverride = -1f)
        {
            currentDelay = durationOverride < 0f ? respawnDelay : durationOverride;
            yield return new WaitForSeconds(currentDelay);

            canTake = false;

            currentDelay = despawnTime;
            var startScale = transform.localScale;

            while (currentDelay > 0f)
            {
                currentDelay -= Time.fixedDeltaTime;
                var lerpFactor = despawnAnim.Evaluate(currentDelay / despawnTime);
                transform.localScale = Vector3.Lerp(Vector3.zero, startScale, lerpFactor);
                yield return new WaitForFixedUpdate();
            }

            if (despawnVfxPrefab != null)
            {
                Instantiate(despawnVfxPrefab, transform.position, Quaternion.identity);
            }

            rigidbodyComponent.velocity = Vector3.zero;
            rigidbodyComponent.angularVelocity = Vector3.zero;

            transform.SetPositionAndRotation(spawnLocation, spawnRotation);
            transform.localScale = spawnScale;

            canTake = true;
        }
    }
}
