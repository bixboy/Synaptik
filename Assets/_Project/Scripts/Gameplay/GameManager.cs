using System.Collections.Generic;
using UnityEngine;

public struct Mission
{
    public string Name;
    public bool IsFinish;
    public IInteraction UfoRef;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    
    [SerializeField] private List<Mission> UfoList { get; set; } = new List<Mission>();
    
    private float Mistrust { get; set; } = 0f;
    private float MaxMistrust = 0f;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearAll()
    {
        UfoList.Clear();
        Mistrust = 0f;
    }
    
    public bool RegisterMission(Mission mission)
    {
        foreach (var ufo in UfoList)
        {
            if (ufo.Name == mission.Name)
            {
                print("Mission already registered");
                return false;   
            }
        }
        
        UfoList.Add(mission);
        return true;
    }

    public void SetMissionFinish(string missionName)
    {
        for (int i = 0; i < UfoList.Count; i++)
        {
            if (UfoList[i].Name == missionName)
            {
                var ufo = UfoList[i];
                
                ufo.IsFinish = true;
                UfoList[i] = ufo;
            }
        }
    }

    public void SetMistrustValue(float value)
    {
        Mistrust = value;

        if (Mistrust >= MaxMistrust)
        {
            print("Defeat");
        }
    }
}
