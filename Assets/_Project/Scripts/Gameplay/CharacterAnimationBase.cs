using System;
using UnityEngine;

public abstract class CharacterAnimationBase : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Rigidbody _rb;

    [Header("Speed Settings")]
    [SerializeField, Min(0f)] protected float _maxReportedSpeed = 8f;
    [SerializeField, Range(0f, 0.5f)] protected float _speedDampTime = 0.1f;

    [Header("Animator Parameter Names")]
    [SerializeField] protected string _paramSpeed = "Speed";
    [SerializeField] protected string _paramIsAngry = "IsAngry";
    [SerializeField] protected string _paramIsCurious = "IsCurious";
    [SerializeField] protected string _paramIsHappy = "IsHappy";
    [SerializeField] protected string _paramIsAfraid = "IsAfraid";
    [SerializeField] protected string _paramHitTrig = "Punch";

    // Hashes
    protected int _hashSpeed;
    protected int _hashIsAngry;
    protected int _hashIsCurious;
    protected int _hashIsHappy;
    protected int _hashIsAfraid;
    protected int _hashHitTrig;

    protected virtual void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected virtual void Awake()
    {
        if (!_animator)
            Debug.LogError($"{GetType().Name}: pas d'Animator assigné !", this);

        _hashSpeed = Animator.StringToHash(_paramSpeed);
        _hashIsAngry = Animator.StringToHash(_paramIsAngry);
        _hashIsCurious = Animator.StringToHash(_paramIsCurious);
        _hashIsHappy = Animator.StringToHash(_paramIsHappy);
        _hashIsAfraid = Animator.StringToHash(_paramIsAfraid);
        _hashHitTrig = Animator.StringToHash(_paramHitTrig);

        if (_rb)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    protected virtual void Update()
    {
        float speed = 0f;
        if (_rb)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            speed = v.magnitude;
        }

        speed = Mathf.Min(speed, _maxReportedSpeed);
        float normalized = _maxReportedSpeed > 0.0001f ? speed / _maxReportedSpeed : 0f;
        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);
    }

    public virtual void SetEmotion(Emotion emotion)
    {
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
        {
            int hash = GetEmotionHash(e);
            if (hash == -1) continue;
            _animator.SetBool(hash, e == emotion);
        }
    }

    public virtual void UnsetEmotion(Emotion emotion)
    {
        int hash = GetEmotionHash(emotion);
        if (hash == -1) return;
        _animator.SetBool(hash, false);
    }

    public virtual void ClearAllEmotions()
    {
        foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
        {
            int hash = GetEmotionHash(e);
            if (hash == -1) continue;
            _animator.SetBool(hash, false);
        }
    }

    protected int GetEmotionHash(Emotion emotion)
    {
        return emotion switch
        {
            Emotion.None => -1,
            Emotion.Anger => _hashIsAngry,
            Emotion.Curious => _hashIsCurious,
            Emotion.Friendly => _hashIsHappy,
            Emotion.Fearful => _hashIsAfraid,
            _ => throw new ArgumentOutOfRangeException(nameof(emotion), emotion, null)
        };
    }

    public virtual void PlayPunch() => _animator.SetTrigger(_hashHitTrig);
}
