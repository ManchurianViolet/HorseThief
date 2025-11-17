using System.Collections;
using UnityEngine;

public class FadeSystem : MonoBehaviour
{
    public static FadeSystem Instance => _instance;
    static FadeSystem _instance;

    CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime;


    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f; // Ensure alpha is exactly 1 at the end
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    public void FadeInAndOut()
    {
        StartCoroutine(FadeOutAndInCoroutine());
    }

    private IEnumerator FadeOutAndInCoroutine()
    {
        yield return StartCoroutine(FadeInCoroutine());  // Then fade in
        yield return new WaitForSecondsRealtime(1.2f);
        yield return StartCoroutine(FadeOutCoroutine()); // Fade out first
    }
}
