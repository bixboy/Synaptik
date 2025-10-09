using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("Références")]
    private LoadingText loadingText;
    [SerializeField] private GameObject loadingScreenPrefab;

    [Header("Paramètres")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDurationBeforeLoad = 0.5f;
    [SerializeField] private float holdDurationAfterSceneLoaded = 0.5f;

    [Header("Text")]
    public List<string> TextFiller = new List<string>();

    private CanvasGroup currentCanvasGroup;
    private GameObject currentLoadingInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
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
            yield break;
        }

        currentCanvasGroup.alpha = 0f;

        if (loadingText)
            loadingText.StartText(TextFiller);

        float estimatedTotalDuration = fadeDuration * 2 + holdDurationBeforeLoad + holdDurationAfterSceneLoaded;
        StartCoroutine(UpdateLoadingProgress(estimatedTotalDuration));

        yield return StartCoroutine(Fade(0f, 1f));

        yield return new WaitForSeconds(holdDurationBeforeLoad);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            if (loadingText)
                loadingText.Loading(asyncLoad.progress);
            
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

       yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);

        yield return new WaitForSeconds(holdDurationAfterSceneLoaded);

       yield return StartCoroutine(Fade(1f, 0f));

        Destroy(currentLoadingInstance);
        currentLoadingInstance = null;
        currentCanvasGroup = null;
    }

    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            if (currentCanvasGroup)
                currentCanvasGroup.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            yield return null;
        }

        if (currentCanvasGroup)
            currentCanvasGroup.alpha = to;
    }

    private IEnumerator UpdateLoadingProgress(float duration)
    {
        if (!loadingText) 
            yield break;

        duration *= 0.9f;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / duration);
            loadingText.Loading(progress);
            
            yield return null;
        }

        loadingText.Loading(1f);
    }
}
