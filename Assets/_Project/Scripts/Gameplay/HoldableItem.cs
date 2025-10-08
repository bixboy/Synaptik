using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class HoldableItem : MonoBehaviour
{
    
    private Rigidbody _rb;
    private Collider[] _colliders;
    private Transform _originalParent;

    public bool IsHeld { get; private set; }
    
    [SerializeField] private string _itemId;
    public string ItemId => _itemId;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>(true);
    }

    public void Pick(Transform handSocket)
    {
        if (IsHeld) return;

        IsHeld = true;
        _originalParent = transform.parent;

        // stop physique et collisions pendant la prise en main
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _rb.useGravity = false;
        foreach (var c in _colliders) c.enabled = false;

        // attache à la main (pose exacte via offsets)
        transform.SetParent(handSocket, worldPositionStays: false);
        transform.localPosition = handSocket.localPosition;
    }

    public void Drop(Vector3 inheritVelocity)
    {
        if (!IsHeld) return;

        // détache et réactive la physique
        transform.SetParent(_originalParent, worldPositionStays: true);
        foreach (var c in _colliders) c.enabled = true;

        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearVelocity = inheritVelocity;

        IsHeld = false;
    }
}