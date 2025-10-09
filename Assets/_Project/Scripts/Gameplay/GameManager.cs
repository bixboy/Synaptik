using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mission associée à un UFO.
/// </summary>
public struct Mission
{
    public string MissionID;
    public string Title;
    public string Description;
    public bool IsFinished;
    
    public Mission(string missionID, string title, string description)
    {
        MissionID = missionID;
        Title = title;
        Description = description;
        IsFinished = false;
    }
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsInitialized { get; private set; } = false;


    // --- Données des missions ---
    [SerializeField] 
    private List<Mission> _missions = new List<Mission>();
    
    
    
    public delegate void TaskEndHandler(Mission mission);
    public event TaskEndHandler OnTaskEnd;

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
        IsInitialized = true;
    }

    // --- Gestion des missions ---
    public bool RegisterMission(Mission mission)
    {
        foreach (var existingMission in _missions)
        {
            if (existingMission.MissionID == mission.MissionID)
            {
                Debug.LogWarning($"Mission '{mission.MissionID}' already registered.");
                return false;
            }
        }

        _missions.Add(mission);
        return true;
    }

    public void SetMissionFinished(string missionName)
    {
        for (int i = 0; i < _missions.Count; i++)
        {
            if (_missions[i].MissionID == missionName)
            {
                var mission = _missions[i];
                mission.IsFinished = true;
                _missions[i] = mission;
                
                OnTaskEnd?.Invoke(mission);

                Debug.Log($"Mission '{missionName}' terminée !");
                return;
            }
        }

        Debug.LogWarning($"Aucune mission trouvée avec le nom '{missionName}'.");
    }

    public void ClearAll()
    {
        foreach (var mission in _missions)
        {
            // if (mission.UfoRef is UFO ufo)
            //     ufo.OnUfoInteract -= HandleUfoInteract;
        }

        _missions.Clear();
        MistrustManager.Instance.RemoveMistrust(1000);

        Debug.Log("Toutes les missions et abonnements ont été nettoyés.");
    }

    public List<Mission> GetMissions()
    {
        return _missions;
    }
}
