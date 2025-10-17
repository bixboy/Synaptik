using UnityEngine;

public class PlayerAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramIsGrabbing = "IsGrabbing";
    private int _hashIsGrabbing;

    protected override void Awake()
    {
        base.Awake();
        _hashIsGrabbing = Animator.StringToHash(_paramIsGrabbing);
    }

    public void SetGrabbing(bool grabbing)
        => _animator.SetBool(_hashIsGrabbing, grabbing);

    public void OnPickedUpItem() => SetGrabbing(true);
    public void OnDroppedItem() => SetGrabbing(false);
}