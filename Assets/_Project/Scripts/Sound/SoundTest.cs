using System;
using UnityEngine;
using FMODUnity;

public class SoundTest : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter _soundEmitter;
    [SerializeField] private StudioBankLoader _bankLoader;

    private void Start()
    {
        if (_bankLoader != null)
        {
            _bankLoader.Load();
        }
        
        if(_soundEmitter == null)
            Debug.Log("FMOD Sound Emitter is null");
        else
        {
            _soundEmitter.Play();
        }
    }
}
