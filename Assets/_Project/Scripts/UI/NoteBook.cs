using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NoteBook : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform missionListContainer;
    [SerializeField] private GameObject missionEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;

    private List<Mission> _missions = new List<Mission>();

    private IEnumerator Start()
    {
        // Attendre que le GameManager soit prêt
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsInitialized);

        GameManager.Instance.OnTaskEnd += HandleTaskEnd;
        GameManager.Instance.OnMissionRegistered += HandleMissionRegistered;
        _missions = GameManager.Instance.GetMissions();
        RefreshNotebookUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnTaskEnd -= HandleTaskEnd;
            GameManager.Instance.OnMissionRegistered -= HandleMissionRegistered;
        }
    }

    private void HandleTaskEnd(Mission mission)
    {
       // _missions = GameManager.Instance.GetMissions();
        RefreshNotebookUI();

    }

    private void HandleMissionRegistered(Mission mission)
    {
        RefreshNotebookUI();
    }

    private void RefreshNotebookUI()
    {
        foreach (Transform child in missionListContainer)
            Destroy(child.gameObject);

        foreach (var mission in _missions)
        {
            GameObject entry = Instantiate(missionEntryPrefab, missionListContainer);
            NotebookEntry notebookEntry = entry.GetComponentInChildren<NotebookEntry>();

            if (notebookEntry != null)
            {
                notebookEntry.Initialize(mission);
                notebookEntry.SetToggle(mission.IsFinished);
            }
        }

        if (scrollRect)
            scrollRect.verticalNormalizedPosition = 1f;
    }
}