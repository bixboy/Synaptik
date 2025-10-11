using Synaptik.Interfaces;
using UnityEngine;

namespace Synaptik.Gameplay
{
    public sealed class AnimalItem : MonoBehaviour, IInteraction
    {
        [SerializeField]
        private GameObject itemToSpawnOnDeath;

        public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
        {
            if (action._behavior == Behavior.Action && action._emotion == Emotion.Anger)
            {
                Die();
            }
        }

        private void Die()
        {
            if (itemToSpawnOnDeath != null)
            {
                Instantiate(itemToSpawnOnDeath, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
