using System;
using System.Collections;
using UnityEngine;

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

        Debug.Log($"[HoldableItem] '{name}' initialisé à la position {spawnLocation}.");
    }

    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (action._behavior != Behavior.Action || playerInteraction == null)
        {
            Debug.Log($"[HoldableItem] Interaction ignorée pour '{name}' : action {action._behavior}, playerInteraction null={(playerInteraction == null)}.");
            return;
        }

        switch (action._emotion)
        {
            case Emotion.Curious:
                Debug.Log($"[HoldableItem] '{name}' interaction curieuse détectée, tentative de ramassage.");
                playerInteraction.PickUp();
                break;
            case Emotion.Friendly when item != null:
                Debug.Log($"[HoldableItem] '{name}' interaction amicale avec objet '{item.name}', tentative de drop.");
                playerInteraction.DropItem();
                break;
        }
    }

    public void Pick(Transform handSocket)
    {
        if (IsHeld || !canTake)
        {
            Debug.LogWarning($"[HoldableItem] Tentative de ramassage invalide pour '{name}' (IsHeld={IsHeld}, canTake={canTake}).");
            return;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            Debug.Log($"[HoldableItem] '{name}' ramassé avant fin de respawn, coroutine arrêtée.");
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
        Debug.Log($"[HoldableItem] '{name}' ramassé par '{handSocket.name}'.");
    }

    public void Drop(Vector3 inheritVelocity)
    {
        if (!IsHeld)
        {
            Debug.LogWarning($"[HoldableItem] Tentative de drop alors que '{name}' n'est pas tenu.");
            return;
        }

        respawnCoroutine = StartCoroutine(Respawn());

        Debug.Log($"[HoldableItem] '{name}' lâché avec vitesse héritée {inheritVelocity}.");

        transform.SetParent(originalParent, true);
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.useGravity = true;
        rigidbodyComponent.linearVelocity = inheritVelocity;

        IsHeld = false;
    }

    private IEnumerator Respawn(float durationOverride = -1f)
    {
        currentDelay = durationOverride < 0f ? respawnDelay : durationOverride;
        Debug.Log($"[HoldableItem] Respawn de '{name}' démarré. Délai: {currentDelay}s.");
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

        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(spawnLocation, spawnRotation);
        transform.localScale = spawnScale;

        canTake = true;
        Debug.Log($"[HoldableItem] '{name}' réinitialisé à sa position d'origine et à nouveau disponible.");
    }
}
