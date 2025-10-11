using UnityEngine;

public class DestructibleItem : MonoBehaviour, IInteraction
{ 
    [Header("Destruction Settings")]
    [SerializeField] private GameObject destructionEffect;

    [SerializeField] private AudioClip destructionSound;

    [SerializeField] private float destroyDelay = 0.1f;

    [SerializeField] private float shakeIntensity = 1f;

    private bool _isDestroyed = false;

    public void Interact(ActionValues action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (_isDestroyed)
            return;
        
        if (action._behavior == Behavior.Action && action._emotion == Emotion.Anger)
        {
            StartCoroutine(DestroySequence());
        }
    }

    private System.Collections.IEnumerator DestroySequence()
    {
        _isDestroyed = true;

        Vector3 startPos = transform.position;
        float timer = 0f;
        float duration = 0.1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float shake = Mathf.Sin(timer * 100f) * shakeIntensity * (1f - timer / duration);
            transform.position = startPos + Random.insideUnitSphere * shake;
            
            yield return null;
        }

        transform.position = startPos;

        if (destructionEffect)
            Instantiate(destructionEffect, transform.position, Quaternion.identity);

        if (destructionSound)
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
    
}
