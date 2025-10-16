using UnityEngine;
using FMODUnity;

public sealed class AnimalItem : MonoBehaviour, IInteraction
{
    [SerializeField]
    private GameObject itemToSpawnOnDeath;

    [Space(7)]
    [SerializeField] private StudioEventEmitter _soundEmitter;
    [Space(5)]
    [SerializeField] private EventReference _soundReaction;
    [SerializeField] private EventReference _soundDeath;

    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (action._behavior == Behavior.Action && action._emotion == Emotion.Anger)
        {
            if (_soundEmitter)
            {
                _soundEmitter.EventReference = _soundDeath;
                _soundEmitter.Play();
            }
            else
                Debug.LogError($"Sound Emitter missing : {gameObject.name}", gameObject);
            
            Die();
        }
        else
        {
            if (_soundEmitter)
            {
                _soundEmitter.EventReference = _soundReaction;
                _soundEmitter.Play();
            }
            else
                Debug.LogError($"Sound Emitter missing : {gameObject.name}", gameObject);
        }
    }

    private void Die()
    {
        if (itemToSpawnOnDeath)
        {
            Instantiate(itemToSpawnOnDeath, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
