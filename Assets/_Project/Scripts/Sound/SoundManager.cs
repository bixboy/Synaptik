using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [SerializeField] private StudioBankLoader _Bank;
    
    [Space(7)]
    [SerializeField] private StudioEventEmitter _ambiantEmitter;
    [SerializeField] private StudioEventEmitter _musicEmitter;
    
    [Space(7)]
    [SerializeField] private StudioEventEmitter _UIValid;
    [SerializeField] private StudioEventEmitter _UIInvalid;
    [Space(5)]
    [SerializeField] private StudioEventEmitter _connectCables;
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Constant Sounds
    public void AmbiantPlay()
    {
        if (_ambiantEmitter != null && !_ambiantEmitter.IsPlaying())
            _ambiantEmitter.Play();
    }
    public void AmbiantStop()
    {
        if (_ambiantEmitter != null && _ambiantEmitter.IsPlaying())
            _ambiantEmitter.Stop();
    }
    
    public void MusicPlay()
    {
        if (_musicEmitter != null && !_musicEmitter.IsPlaying())
            _musicEmitter.Play();
    }
    public void MusicStop()
    {
        if (_musicEmitter != null && _musicEmitter.IsPlaying())
            _musicEmitter.Stop();
    }
    #endregion
    
    #region Recurrent One Shots

    public void UIValid()
    {
        if (_UIValid != null)
            _UIValid.Play();
    }
    public void UIInvalid()
    {
        if (_UIInvalid != null)
            _UIInvalid.Play();
    }

    public void ConnectCables()
    {
        if (_connectCables != null)
            _connectCables.Play();
    }
    #endregion
}
