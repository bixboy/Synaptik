using UnityEngine;
using UnityEngine.Events;

public class VFXPlayer : MonoBehaviour
{
    [SerializeField] private UnityEvent onVFXPlayed;
    [SerializeField] private UnityEvent onVFXStopped;
    [SerializeField] private UnityEvent onVFXPaused;
    
    public void PlayVFX()
    {
        onVFXPlayed?.Invoke();
    }
    public void StopVFX()
    {
        onVFXStopped?.Invoke();
    }
    public void PauseVFX()
    {
        onVFXPaused?.Invoke();
    }
}
