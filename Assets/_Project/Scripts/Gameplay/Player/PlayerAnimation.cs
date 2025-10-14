using System;
using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody _rb; // optionnel mais recommandé
    

    [Header("Speed Settings")]
    [SerializeField, Min(0f), Tooltip("Clamp pour l'anim")]
    private float _maxReportedSpeed = 8f; // clamp pour l'anim
    [SerializeField, Range(0f, 0.5f)] private float _speedDampTime = 0.1f;

    [Header("Animator Parameter Names (override si besoin)")]
    [SerializeField] private string _paramSpeed      = "Speed";       
    [SerializeField] private string _paramIsGrabbing = "IsGrabbing";  
    [SerializeField] private string _paramIsAngry    = "IsAngry";      
    [SerializeField] private string _paramIsCurious  = "IsCurious"; 
    [SerializeField] private string _paramIsHappy  = "IsHappy";
    [SerializeField] private string _paramIsAfraid  = "IsAfraid";
    [SerializeField] private string _paramHitTrig    = "Hit";         
    [SerializeField] private string _paramPukeTrig   = "Puke";        

    // Hashes (no string alloc per-frame)
    private int _hashSpeed;
    private int _hashIsGrabbing;
    private int _hashIsAngry;
    private int _hashIsCurious;
    private int _hashIsHappy;
    private int _hashIsAfraid;
    private int _hashHitTrig;
    private int _hashPukeTrig;
   
    
    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (!_animator) Debug.LogError("PlayerAnimation: pas d'Animator assigné !", this);
        _hashSpeed      = Animator.StringToHash(_paramSpeed);
        _hashIsGrabbing = Animator.StringToHash(_paramIsGrabbing);
        _hashIsAngry    = Animator.StringToHash(_paramIsAngry);
        _hashIsCurious  = Animator.StringToHash(_paramIsCurious);
        _hashIsHappy    = Animator.StringToHash(_paramIsHappy);
        _hashIsAfraid   = Animator.StringToHash(_paramIsAfraid);
        _hashHitTrig    = Animator.StringToHash(_paramHitTrig);
        _hashPukeTrig   = Animator.StringToHash(_paramPukeTrig);
        

        // Qualité : éviter de bouger/rotations physiques selon ton setup
        if (_rb)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        float speed = 0f;
        if (_rb)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            speed = v.magnitude;
        }

        // 2) Clamp et normalisation si ton controller s’attend à 0..1
        speed = Mathf.Min(speed, _maxReportedSpeed);
        float normalized = _maxReportedSpeed > 0.0001f ? speed / _maxReportedSpeed : 0f;
        _animator.SetFloat(_hashSpeed, normalized, _speedDampTime, Time.deltaTime);
    }
    
    
    public void SetEmotion(Emotion emotion)
    {
        foreach (Emotion _emotion in Enum.GetValues(typeof(Emotion)))
        {
            if (_emotion == emotion)  
                _animator.SetBool(GetEmotionHash(_emotion), true);
            else
                _animator.SetBool(GetEmotionHash(_emotion), false);
        }
    }
    
    private int GetEmotionHash(Emotion emotion)
    {
        return emotion switch
        {
            Emotion.Anger   => _hashIsAngry,
            Emotion.Curious => _hashIsCurious,
            Emotion.Friendly   => _hashIsHappy,
            Emotion.Fearful  => _hashIsAfraid,
            _               => throw new ArgumentOutOfRangeException(nameof(emotion), emotion, null)
        };
    }
    
    public void PlayPunch()
        => _animator.SetTrigger(_hashHitTrig);
    
    public void PlayPuke()
        => _animator.SetTrigger(_hashPukeTrig);
    
    public void SetGrabbing(bool grabbing)
        => _animator.SetBool(_hashIsGrabbing, grabbing);

    public void OnPickedUpItem()  => SetGrabbing(true);
    public void OnDroppedItem()   => SetGrabbing(false);
}
