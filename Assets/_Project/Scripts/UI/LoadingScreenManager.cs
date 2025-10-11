using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synaptik.UI
{
    public sealed class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance { get; private set; }

        [Header("Références")]
        [SerializeField]
        private GameObject loadingScreenPrefab;

        [Header("Paramètres")]
        [SerializeField]
        private float fadeDuration = 1f;

        [SerializeField]
        private float holdDurationBeforeLoad = 0.5f;

        [SerializeField]
        private float holdDurationAfterSceneLoaded = 0.5f;

        [Header("Text")]
        public List<string> textFiller = new();

        private LoadingText loadingText;
        private CanvasGroup currentCanvasGroup;
        private GameObject currentLoadingInstance;

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

        public void LoadScene(string sceneName)
        {
            if (currentLoadingInstance != null)
            {
                Debug.LogWarning("Une scène est déjà en cours de chargement.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            currentLoadingInstance = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(currentLoadingInstance);

            currentCanvasGroup = currentLoadingInstance.GetComponentInChildren<CanvasGroup>();
            loadingText = currentLoadingInstance.GetComponentInChildren<LoadingText>();

            if (currentCanvasGroup == null)
            {
                Debug.LogError("Le prefab de loading screen doit contenir un CanvasGroup !");
                CleanupLoadingInstance();
                yield break;
            }

            currentCanvasGroup.alpha = 0f;

            loadingText?.StartText(textFiller);

            var estimatedTotalDuration = fadeDuration * 2 + holdDurationBeforeLoad + holdDurationAfterSceneLoaded;
            StartCoroutine(UpdateLoadingProgress(estimatedTotalDuration));

            yield return StartCoroutine(Fade(0f, 1f));
            yield return new WaitForSeconds(holdDurationBeforeLoad);

            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                loadingText?.SetLoadingProgress(asyncLoad.progress);
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;

            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);
            yield return new WaitForSeconds(holdDurationAfterSceneLoaded);
            yield return StartCoroutine(Fade(1f, 0f));

            CleanupLoadingInstance();
        }

        private IEnumerator Fade(float from, float to)
        {
            var time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                if (currentCanvasGroup != null)
                {
                    currentCanvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
                }

                yield return null;
            }

            if (currentCanvasGroup != null)
            {
                currentCanvasGroup.alpha = to;
            }
        }

        private IEnumerator UpdateLoadingProgress(float duration)
        {
            if (loadingText == null)
            {
                yield break;
            }

            duration *= 0.9f;

            var time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                var progress = Mathf.Clamp01(time / duration);
                loadingText.SetLoadingProgress(progress);

                yield return null;
            }

            loadingText.SetLoadingProgress(1f);
        }

        private void CleanupLoadingInstance()
        {
            if (currentLoadingInstance != null)
            {
                Destroy(currentLoadingInstance);
            }

            currentLoadingInstance = null;
            currentCanvasGroup = null;
            loadingText = null;
        }
    }
}
