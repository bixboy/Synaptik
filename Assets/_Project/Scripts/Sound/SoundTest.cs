using UnityEngine;
using System;
using FMOD;
using FMODUnity;
using Debug = UnityEngine.Debug;


public class FMODOutputSelector : MonoBehaviour
{
    
    public int audioIndex = 0;
    [SerializeField] FMODUnity.StudioEventEmitter emitter;
    
    void Start()
    {
        string name = new string(' ', 256);
        int numDrivers;
        RuntimeManager.CoreSystem.getNumDrivers(out numDrivers);

        for (int i = 0; i < numDrivers; i++)
        {
            RuntimeManager.CoreSystem.getDriverInfo(i, out name, name.Length, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakerChannel);
            Debug.Log($"Output {i}: {name}");
        }

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 50, 30), "Test Audio"))
        {
            RuntimeManager.CoreSystem.getNumDrivers(out int numDrivers);

            if (audioIndex >= numDrivers || audioIndex < 0)
            {
                Debug.Log($"Audio Driver index out of range");
                return;
            }
            
            FMOD.System system;
            FMOD.Factory.System_Create(out system);
            
            system.setDriver(audioIndex);
            system.init(32, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);

            FMOD.Sound sound;
            system.createSound("Computer_Bip_SFX_OS.wav", FMOD.MODE.DEFAULT, out sound);
            system.playSound(sound, new ChannelGroup(), false, out _);
            
            Debug.Log($"Sound player On : {audioIndex} - {sound}");
            
            // A Marche Po :c
        }
    }
}
