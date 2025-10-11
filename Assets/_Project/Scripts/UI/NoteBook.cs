using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Synaptik.UI
{
    public sealed class NoteBook : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private Transform missionListContainer;

        [SerializeField]
        private GameObject missionEntryPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        private readonly List<Gameplay.Mission> missions = new();
        private Gameplay.GameManager gameManager;

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
            gameManager = Gameplay.GameManager.Instance;
            return gameManager != null && gameManager.IsInitialized;
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnTaskEnd -= HandleTaskEnd;
            }
        }

        private void HandleTaskEnd(Gameplay.Mission mission)
        {
            SyncMissions();
            RefreshNotebookUI();
        }

        private void SyncMissions()
        {
            missions.Clear();

            if (gameManager == null)
            {
                return;
            }

            missions.AddRange(gameManager.GetMissions());
        }

        private void RefreshNotebookUI()
        {
            if (missionListContainer == null || missionEntryPrefab == null)
            {
                return;
            }

            for (var i = missionListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(missionListContainer.GetChild(i).gameObject);
            }

            foreach (var mission in missions)
            {
                var entry = Instantiate(missionEntryPrefab, missionListContainer);
                var notebookEntry = entry.GetComponentInChildren<NotebookEntry>();

                if (notebookEntry == null)
                {
                    continue;
                }

                notebookEntry.Initialize(mission);
                notebookEntry.SetToggle(mission.IsFinished);
            }

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
