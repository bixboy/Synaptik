using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class HoldableItem : MonoBehaviour, IInteraction
{
    private const string LogPrefix = "[HoldableItem]";

    [SerializeField]
    private string itemId;

    [Header("Respawn")]
    [SerializeField]
    private float respawnDelay = 5f;

    [SerializeField]
    private float despawnTime = 0.5f;

    [SerializeField]
    private AnimationCurve despawnAnim = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private bool respawnAtDrop = false;

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
    [SerializeField] private bool canTake = true;

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

        Debug.Log($"{LogPrefix} '{name}' prêt à {spawnLocation}.");
    }

    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (playerInteraction == null)
        {
            Debug.LogWarning($"{LogPrefix} Interaction ignorée sur '{name}' (player manquant).");
            return;
        }

        if (action._behavior != Behavior.Action)
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
            Debug.LogWarning($"{LogPrefix} Ramassage invalide pour '{name}' (IsHeld={IsHeld}, CanTake={canTake}).");
            return;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        IsHeld = true;
        originalParent = transform.parent;

        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;
        rigidbodyComponent.isKinematic = true;
        rigidbodyComponent.useGravity = false;

        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        transform.SetParent(handSocket, false);
        transform.localPosition = handSocket.localPosition;
        Debug.Log($"{LogPrefix} '{name}' ramassé par '{handSocket.name}'.");
    }

    public void Drop(Vector3 inheritVelocity)
    {
        if (!IsHeld)
        {
            Debug.LogWarning($"{LogPrefix} Tentative de drop alors que '{name}' n'est pas tenu.");
            return;
        }

        if (respawnAtDrop)
        {
            spawnLocation = transform.position;
            spawnRotation = transform.rotation;
            spawnScale = transform.localScale;

            respawnAtDrop = false;
        }

        respawnCoroutine = StartCoroutine(Respawn());

        transform.SetParent(originalParent, true);
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.useGravity = true;
        rigidbodyComponent.linearVelocity = inheritVelocity;

        IsHeld = false;
        Debug.Log($"{LogPrefix} '{name}' lâché.");
    }

    private IEnumerator Respawn(float durationOverride = -1f)
    {
        currentDelay = durationOverride < 0f ? respawnDelay : durationOverride;
        Debug.Log($"{LogPrefix} Respawn de '{name}' démarré ({currentDelay:F1}s).");
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

        SetAtSpawn();
    }

    public void SetAtSpawn()
    {
        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(spawnLocation, spawnRotation);
        transform.localScale = spawnScale;
        
        transform.SetParent(originalParent, true);
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.useGravity = true;

        IsHeld = false;
        canTake = true;
        Debug.Log($"{LogPrefix} '{name}' réinitialisé et disponible.");
    }
}
