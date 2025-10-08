using System;
using System.Collections;
using UnityEngine;

public class VFXDestroyer : MonoBehaviour
{
    public float _VFXLifeTime = 1.5f;

    private void Start()
    {
        StartCoroutine(DestroyVFX());
    }

    private IEnumerator DestroyVFX()
    {
        yield return new WaitForSeconds(_VFXLifeTime);
        Destroy(gameObject);
    }
}
