using System.Collections;
using UnityEngine;

public sealed class VFXDestroyer : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 1.5f;

    private void Start()
    {
        StartCoroutine(DestroyVFX());
    }

    private IEnumerator DestroyVFX()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
