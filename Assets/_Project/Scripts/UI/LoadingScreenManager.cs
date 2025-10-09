using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("Références")]
    [SerializeField] private GameObject loadingScreenPrefab;

    [Header("Paramètres")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDurationBeforeLoad = 0.5f;  // temps avant de commencer le chargement
    [SerializeField] private float holdDurationAfterSceneLoaded = 0.5f; // temps après activation de la nouvelle scène avant le fade-out

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
        // --- Instanciation du prefab ---
        currentLoadingInstance = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(currentLoadingInstance); // 👈 garder l’écran de loading pendant le changement de scène
        currentCanvasGroup = currentLoadingInstance.GetComponentInChildren<CanvasGroup>();

        if (!currentCanvasGroup)
        {
            Debug.LogError("Le prefab de loading screen doit contenir un CanvasGroup !");
            yield break;
        }

        currentCanvasGroup.alpha = 0f;

        // --- FADE-IN ---
        yield return StartCoroutine(Fade(0f, 1f));

        // --- MAINTIEN avant de commencer le chargement ---
        yield return new WaitForSeconds(holdDurationBeforeLoad);

        // --- CHARGEMENT ASYNCHRONE ---
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // --- ACTIVER LA NOUVELLE SCÈNE ---
        asyncLoad.allowSceneActivation = true;

        // Attendre que la nouvelle scène soit effectivement chargée et active
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);

        // --- MAINTIEN APRÈS LE CHARGEMENT ---
        yield return new WaitForSeconds(holdDurationAfterSceneLoaded);

        // --- FADE-OUT une fois la nouvelle scène affichée ---
        yield return StartCoroutine(Fade(1f, 0f));

        // --- DÉTRUIRE le prefab ---
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
}
