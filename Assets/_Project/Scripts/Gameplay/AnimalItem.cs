using Synaptik.Game;
using UnityEngine;


public class AnimalItem : MonoBehaviour, IInteraction
{
    [SerializeField] private GameObject _ItemToSpawnOnDeath;
    
    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        Behavior behavior = action._behavior;
        Emotion emotion = action._emotion;
        if (behavior == Behavior.Action)
        {
            if (emotion == Emotion.Anger)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        Instantiate(_ItemToSpawnOnDeath, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
