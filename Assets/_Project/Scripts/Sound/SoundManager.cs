using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

#region Struct&Enum

[System.Serializable]
struct SoundWithEmotion
{
    public Emotion emotion;
    public EventReference eventReference;
}
[System.Serializable]
struct SoundWithMission
{
    public string missionID;
    public EventReference eventReference;
}

[System.Serializable]
public enum VoicesModels
{
    RAND = 0,
    
    VOICE_A = 1,
    VOICE_T = 2,
    VOICE_P = 3
}

[System.Serializable]
public enum AudioReaction
{
    None = 0,
    
    Angry = 1,
    Curious = 2,
    Friendly = 3,
    Sad = 4,
    Scared = 5
}

#endregion

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [SerializeField] private StudioBankLoader _Bank;
    
    [Space(7)]
    [SerializeField] private StudioEventEmitter _ambiantEmitter;
    [SerializeField] private StudioEventEmitter _musicEmitter;
    [SerializeField] private StudioEventEmitter _cameraSFXEmitter;
    
    [Space(7)]
    [SerializeField] private EventReference _UIValid;
    [SerializeField] private EventReference _UIInvalid;
    [Space(5)]
    [SerializeField] private EventReference _connectCables;
    
    [Space(7)]
    [SerializeField] private List<SoundWithEmotion> _serialVoicesRand = new List<SoundWithEmotion>();
    private Dictionary<Emotion, EventReference> _voicesRand = new Dictionary<Emotion, EventReference>();
    [Space(5)]
    [SerializeField] private List<SoundWithEmotion> _serialVoicesA = new List<SoundWithEmotion>();
    private Dictionary<Emotion, EventReference> _voicesA = new Dictionary<Emotion, EventReference>();
    [SerializeField] private List<SoundWithEmotion> _serialVoicesT = new List<SoundWithEmotion>();
    private Dictionary<Emotion, EventReference> _voicesT = new Dictionary<Emotion, EventReference>();
    [SerializeField] private List<SoundWithEmotion> _serialVoicesP = new List<SoundWithEmotion>();
    private Dictionary<Emotion, EventReference> _voicesP = new Dictionary<Emotion, EventReference>();

    [Space(7)]
    [SerializeField] private List<SoundWithMission> _serialVoicesMission = new List<SoundWithMission>();
    private Dictionary<string, EventReference> _voicesMission = new Dictionary<String, EventReference>();
    
    
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
    private void Start()
    {
        foreach (SoundWithEmotion current in _serialVoicesRand)
        {
            _voicesRand.Add(current.emotion, current.eventReference);
        }
        
        foreach (SoundWithEmotion current in _serialVoicesA)
        {
            _voicesA.Add(current.emotion, current.eventReference);
        }
        foreach (SoundWithEmotion current in _serialVoicesT)
        {
            _voicesT.Add(current.emotion, current.eventReference);
        }
        foreach (SoundWithEmotion current in _serialVoicesP)
        {
            _voicesP.Add(current.emotion, current.eventReference);
        }

        foreach (SoundWithMission current in _serialVoicesMission)
        {
            _voicesMission.Add(current.missionID, current.eventReference);
        }
    }

    #region Constant Sounds

    public bool AmbiantChange(EventReference a_audioEvent, bool a_play = true)
    {
        if (a_audioEvent.IsNull)
            return false;
        
        AmbiantStop();
        _ambiantEmitter.EventReference = a_audioEvent;
        AmbiantPlay();

        return true;
    }
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
    
    public bool MusicChange(EventReference a_audioEvent, bool a_play = true)
    {
        if (a_audioEvent.IsNull)
            return false;
        
        MusicStop();
        _musicEmitter.EventReference = a_audioEvent;
        MusicPlay();

        return true;
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
        FMODUnity.RuntimeManager.PlayOneShot(_UIValid);
    }
    public void UIInvalid()
    {
        FMODUnity.RuntimeManager.PlayOneShot(_UIInvalid);
    }

    public void PlaySFX(EventReference a_sound)
    {
        FMODUnity.RuntimeManager.PlayOneShot(a_sound);
    }

    public void ConnectCables()
    {
        FMODUnity.RuntimeManager.PlayOneShot(_connectCables);
    }
    #endregion
    
    #region Voices

    public EventReference GetVoice(Emotion a_emotion, VoicesModels a_voice = VoicesModels.RAND)
    {
        EventReference eventRef = new EventReference();
        
        switch (a_voice)
        {
            case VoicesModels.VOICE_A:
                if (_voicesA.ContainsKey(a_emotion))
                    eventRef = _voicesA[a_emotion];
                break;
            case VoicesModels.VOICE_P:
                if (_voicesP.ContainsKey(a_emotion))
                    eventRef = _voicesA[a_emotion];
                break;
            case VoicesModels.VOICE_T:
                if (_voicesT.ContainsKey(a_emotion))
                    eventRef = _voicesA[a_emotion];
                break;
            
            case VoicesModels.RAND:
            default:
                if (_voicesRand.ContainsKey(a_emotion))
                    eventRef = _voicesA[a_emotion];
                break;
        }

        return eventRef;
    }

    public EventReference GetMissionVoice(string a_missionID)
    {
        EventReference eventRef = new EventReference();

        if (_voicesMission.ContainsKey(a_missionID))
            eventRef = _voicesMission[a_missionID];

        return eventRef;
    }
    
    #endregion
}
