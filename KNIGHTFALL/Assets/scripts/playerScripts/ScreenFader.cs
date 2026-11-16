
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Fade Settings")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.5f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(FadeToSceneRoutine(sceneName));
    }

    private IEnumerator FadeToSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        // Fade to black.
        fadeGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadeGroup.alpha = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            yield return null;
        }

        fadeGroup.alpha = 1f;

        // Load the destination scene.
        SceneManager.LoadScene(sceneName);

        // Give the new scene a frame to load.
        yield return null;

        // Fade back in.
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadeGroup.alpha = 1f - Mathf.Clamp01(
                elapsed / fadeDuration
            );

            yield return null;
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;

        isTransitioning = false;
    }
}
