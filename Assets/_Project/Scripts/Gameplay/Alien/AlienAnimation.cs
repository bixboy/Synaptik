using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AlienAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramPukeTrig = "Puke";
    private int _hashPukeTrig;

    private CharacterController _controller;
    private Vector3 _lastPosition;

    protected override void Awake()
    {
        base.Awake();
        _hashPukeTrig = Animator.StringToHash(_paramPukeTrig);

        _controller = GetComponent<CharacterController>();
        _lastPosition = transform.position;
    }

    protected override void Update()
    {

        Vector3 delta = transform.position - _lastPosition;
        delta.y = 0f; // ignore les mouvements verticaux

        float speed = delta.magnitude / Time.deltaTime;
        speed = Mathf.Min(speed, _maxReportedSpeed);
        float normalized = _maxReportedSpeed > 0.0001f ? speed / _maxReportedSpeed : 0f;

        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);

        _lastPosition = transform.position;
    }

    public void PlayPuke() => _animator.SetTrigger(_hashPukeTrig);
}