using UnityEngine;

public class AlienAnimation : CharacterAnimationBase
{
    [SerializeField] private string _paramPukeTrig = "Puke";
    private int _hashPukeTrig;

    protected override void Awake()
    {
        base.Awake();
        _hashPukeTrig = Animator.StringToHash(_paramPukeTrig);
    }

    public void PlayPuke() => _animator.SetTrigger(_hashPukeTrig);
}