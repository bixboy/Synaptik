using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private GameObject loadingScreenPrefab;

    [Header("Durées")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDurationBeforeLoad = 0.5f;
    [SerializeField] private float holdDurationAfterSceneLoaded = 0.5f;

    [Header("Texte")]
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

        if (!currentCanvasGroup)
        {
            Debug.LogError("Le prefab de loading screen doit contenir un CanvasGroup !");
            CleanupLoadingInstance();
            yield break;
        }

        currentCanvasGroup.alpha = 0f;
        loadingText?.StartText(textFiller);

        // On calcule la durée totale
        float totalTime = fadeDuration * 2 + holdDurationBeforeLoad + holdDurationAfterSceneLoaded;
        float elapsed = 0f;

        // On crée une fonction locale pour MAJ la progression
        void UpdateProgress()
        {
            float progress = Mathf.Clamp01(elapsed / totalTime);
            loadingText?.SetLoadingProgress(progress);
        }

        // --- Fade In ---
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            if (currentCanvasGroup)
                currentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            UpdateProgress();
            yield return null;
        }

        // --- Hold avant load ---
        t = 0f;
        while (t < holdDurationBeforeLoad)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            UpdateProgress();
            yield return null;
        }

        // --- Load Scene Async ---
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            t = Time.deltaTime;
            elapsed += t;
            UpdateProgress();
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);

        // --- Hold après load ---
        t = 0f;
        while (t < holdDurationAfterSceneLoaded)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            UpdateProgress();
            yield return null;
        }

        // --- Fade Out ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;
            if (currentCanvasGroup)
                currentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            UpdateProgress();
            yield return null;
        }

        // Assure 100 %
        loadingText?.SetLoadingProgress(1f);

        // --- Fin ---
        CleanupLoadingInstance();
    }

    private void CleanupLoadingInstance()
    {
        if (currentLoadingInstance)
            Destroy(currentLoadingInstance);

        currentLoadingInstance = null;
        currentCanvasGroup = null;
        loadingText = null;
    }
}
