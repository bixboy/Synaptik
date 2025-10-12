using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class NoteBook : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private Transform missionListContainer;

    [SerializeField]
    private GameObject missionEntryPrefab;

    [SerializeField]
    private ScrollRect scrollRect;

    private readonly List<Mission> missions = new();
    private GameManager gameManager;

    private IEnumerator Start()
    {
        yield return new WaitUntil(TryCacheGameManager);

        if (gameManager == null)
        {
            yield break;
        }

        gameManager.OnTaskEnd += HandleTaskEnd;
        SyncMissions();
        RefreshNotebookUI();
    }

    private bool TryCacheGameManager()
    {
        gameManager = GameManager.Instance;
        return gameManager != null && gameManager.IsInitialized;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTaskEnd -= HandleTaskEnd;
        }
    }

    private void HandleTaskEnd(Mission mission)
    {
        SyncMissions();
        RefreshNotebookUI();
    }

    private void SyncMissions()
    {
        missions.Clear();

        if (!gameManager)
            return;

        var gmMissions = gameManager.GetMissions();
        Debug.Log($"[Notebook] Found {gmMissions.Count} missions in GameManager.");
        
        missions.AddRange(gmMissions);
    }

    private void RefreshNotebookUI()
    {
        if (!missionListContainer || !missionEntryPrefab)
            return;

        for (var i = missionListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(missionListContainer.GetChild(i).gameObject);
        }

        foreach (var mission in missions)
        {
            var entry = Instantiate(missionEntryPrefab, missionListContainer);
            var notebookEntry = entry.GetComponentInChildren<NotebookEntry>();

            if (!notebookEntry)
                continue;

            notebookEntry.Initialize(mission);
            notebookEntry.SetToggle(mission.IsFinished);
        }

        if (scrollRect)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
