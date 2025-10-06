using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mission associée à un UFO.
/// </summary>
public struct Mission
{
    public string Name;
    public bool IsFinished;
    public IInteraction UfoRef;
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Données des missions ---
    [SerializeField] 
    private List<Mission> _missions = new List<Mission>();

    // --- Paramètres de gameplay ---
    [SerializeField] 
    private float _mistrust = 0f;

    [SerializeField] 
    private float _maxMistrust = 100f;

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

    // --- Gestion des missions ---
    public bool RegisterMission(Mission mission)
    {
        foreach (var existingMission in _missions)
        {
            if (existingMission.Name == mission.Name)
            {
                Debug.LogWarning($"Mission '{mission.Name}' already registered.");
                return false;
            }
        }

        _missions.Add(mission);

        if (mission.UfoRef is UFO ufo)
            ufo.OnUfoInteract += HandleUfoInteract;

        return true;
    }

    public void SetMissionFinished(string missionName)
    {
        for (int i = 0; i < _missions.Count; i++)
        {
            if (_missions[i].Name == missionName)
            {
                var mission = _missions[i];
                mission.IsFinished = true;
                _missions[i] = mission;

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
            if (mission.UfoRef is UFO ufo)
                ufo.OnUfoInteract -= HandleUfoInteract;
        }

        _missions.Clear();
        _mistrust = 0f;

        Debug.Log("Toutes les missions et abonnements ont été nettoyés.");
    }

    // --- Gestion de la méfiance ---
    public void SetMistrustValue(float value)
    {
        _mistrust = Mathf.Clamp(value, 0f, _maxMistrust);

        if (_mistrust >= _maxMistrust)
        {
            Debug.Log("💀 Défaite : méfiance maximale atteinte !");
        }
    }

    // --- Gestion des interactions UFO ---
    private void HandleUfoInteract(UFO ufo, ActionValues action)
    {
        Debug.Log($"[GameManager] Interaction détectée avec : {ufo.transform.parent.name}");
    }
}
