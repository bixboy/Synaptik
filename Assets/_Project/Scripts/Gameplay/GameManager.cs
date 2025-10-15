using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

public sealed class GameManager : MonoBehaviour
{
    private const string LogPrefix = "[GameManager]";

    public static GameManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }

    [SerializeField]
    private List<Mission> missions = new();

    public delegate void TaskEndHandler(Mission mission, AlienDefinition alienDefinition);
    public event TaskEndHandler OnTaskEnd;
    
    public delegate void AllTaskEndHandler();
    public event AllTaskEndHandler OnAllTaskEnd;

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
        Debug.Log($"{LogPrefix} Initialisation terminée.");
    }

    public bool RegisterMission(Mission mission)
    {
        foreach (var existingMission in missions)
        {
            if (existingMission.MissionID != mission.MissionID)
            {
                continue;
            }

            Debug.LogWarning($"{LogPrefix} Mission '{mission.MissionID}' already registered.");
            return false;
        }

        missions.Add(mission);
        Debug.Log($"{LogPrefix} Mission '{mission.MissionID}' enregistrée. Total missions: {missions.Count}.");
        return true;
    }

    public void SetMissionFinished(string missionId, AlienDefinition alienDefinition)
    {
        for (var i = 0; i < missions.Count; i++)
        {
            if (missions[i].MissionID != missionId)
            {
                continue;
            }

            var mission = missions[i];
            if (mission.IsFinished)
            {
                Debug.LogWarning($"{LogPrefix} Mission '{missionId}' déjà terminée.");
                return;
            }

            mission.IsFinished = true;
            missions[i] = mission;

            Debug.Log($"{LogPrefix} Mission '{missionId}' complétée.");
            OnTaskEnd?.Invoke(mission, alienDefinition);
            OnTaskEnd?.Invoke(mission);

            int missionComplete = 0;
            for (int j = 0; j < missions.Count; j++)
            {
                if (missions[j].IsFinished)
                {
                    missionComplete ++;
                }
            }
            
            if (missionComplete == missions.Count)
                OnAllTaskEnd?.Invoke();
            
            return;
        }

        Debug.LogWarning($"{LogPrefix} Aucune mission trouvée avec l'identifiant '{missionId}'.");
    }

    public void ClearAll()
    {
        missions.Clear();
        MistrustManager.Instance?.RemoveMistrust(1000);

        Debug.Log($"{LogPrefix} Toutes les missions et abonnements ont été nettoyés.");
    }

    public IReadOnlyList<Mission> GetMissions()
    {
        return missions;
    }
}
