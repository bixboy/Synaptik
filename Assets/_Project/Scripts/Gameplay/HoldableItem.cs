using System.Collections;
using Synaptik.Game;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class HoldableItem : MonoBehaviour, IInteraction
{
    private Rigidbody _rb;
    private Collider[] _colliders;
    private Transform _originalParent;

    public bool IsHeld { get; private set; }
    
    [SerializeField] private string _itemId;
    public string ItemId => _itemId;
    public bool CanBePicked => _canTake && !IsHeld;
    
    [Header("Respawn")]
    private Vector3 _spawnLocation;
    private Quaternion _spawnRotation;
    private Vector3 _spawnScale;
    [Space(7)]
    [SerializeField] private float _respawnDelay = 5.0f;
    private float _delayCurrent;
    [Space(5)]
    [SerializeField] private float _despawnTime = 0.5f;
    [SerializeField] private AnimationCurve _despawnAnim = AnimationCurve.Linear(0, 0, 1, 1);
    private Coroutine _respawnCoroutine;
    [Space(7)]
    [SerializeField] private GameObject _despawnVFXPrefab;

    private bool _canTake = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>(true);
        
        _spawnLocation = transform.position;
        _spawnRotation = transform.rotation;
        _spawnScale = transform.localScale;
    }
    
    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        Behavior behavior = action._behavior;
        Emotion emotion = action._emotion;
        if (behavior == Behavior.Action)
        {
            if (emotion == Emotion.Curious)
            {
                playerInteraction?.PickUp();
            }
            else if (emotion == Emotion.Friendly)
            {
                if (item) playerInteraction?.DropItem();
            }
        }
    }

    public void Pick(Transform handSocket)
    {
        if (IsHeld || !_canTake) return;
        
        if (_respawnCoroutine != null)
            StopCoroutine(_respawnCoroutine);

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
        
        _respawnCoroutine = StartCoroutine(Respawn());
        
        // détache et réactive la physique
        transform.SetParent(_originalParent, worldPositionStays: true);
        foreach (var c in _colliders) c.enabled = true;

        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearVelocity = inheritVelocity;

        IsHeld = false;
    }

    private IEnumerator Respawn(float a_time = -1.0f)
    {
        if (a_time < 0)
            _delayCurrent = _respawnDelay;
        else
            _delayCurrent = a_time;
        
        yield return new WaitForSeconds(_delayCurrent);

        _canTake = false;
        
        _delayCurrent = _despawnTime;
        Vector3 startScale = transform.localScale;

        while (_delayCurrent > 0)
        {
            _delayCurrent -= Time.fixedDeltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, _despawnAnim.Evaluate(_delayCurrent / _despawnTime));
            yield return new WaitForFixedUpdate();
        }
        
        if (_despawnVFXPrefab != null)
            Instantiate(_despawnVFXPrefab, transform.position, Quaternion.Euler(Vector3.zero));
        
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        transform.position = _spawnLocation;
        transform.rotation = _spawnRotation;
        transform.localScale = _spawnScale;

        _canTake = true;
    }

    
}