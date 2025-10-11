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
    public static GameManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }

    [SerializeField]
    private List<Mission> missions = new();

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
        Debug.Log("[GameManager] Initialisation terminée.");
    }

    public bool RegisterMission(Mission mission)
    {
        foreach (var existingMission in missions)
        {
            if (existingMission.MissionID != mission.MissionID)
            {
                continue;
            }

            Debug.LogWarning($"Mission '{mission.MissionID}' already registered.");
            return false;
        }

        missions.Add(mission);
        Debug.Log($"[GameManager] Mission '{mission.MissionID}' enregistrée (total: {missions.Count}).");
        return true;
    }

    public void SetMissionFinished(string missionId)
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
                Debug.LogWarning($"[GameManager] Mission '{missionId}' déjà marquée comme terminée.");
                return;
            }

            mission.IsFinished = true;
            missions[i] = mission;

            Debug.Log($"[GameManager] Mission '{missionId}' passe en statut terminé.");
            OnTaskEnd?.Invoke(mission);
            Debug.Log($"Mission '{missionId}' terminée !");
            return;
        }

        Debug.LogWarning($"Aucune mission trouvée avec le nom '{missionId}'.");
    }

    public void ClearAll()
    {
        missions.Clear();
        MistrustManager.Instance?.RemoveMistrust(1000);

        Debug.Log("[GameManager] Toutes les missions et abonnements ont été nettoyés.");
    }

    public IReadOnlyList<Mission> GetMissions()
    {
        return missions;
    }
}
