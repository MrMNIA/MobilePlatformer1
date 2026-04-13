using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Eğer yanlışlıkla bir objenin altındaysa kurtarır
            DontDestroyOnLoad(gameObject);

            // CanvasGroup'u otomatik bulalım ki MissingReference riskini azaltalım
            if (canvasGroup == null)
                canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator ManualFadeIn()
    {
        canvasGroup.blocksRaycasts = true; // Geçiş bitene kadar tıklama engellensin
        float t = canvasGroup.alpha;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    // Bu Coroutine'i diğer scriptlerden "yield return" ile çağıracağız
    public IEnumerator ManualFadeOut()
    {
        canvasGroup.blocksRaycasts = true;
        float t = 0;
        while (t < fadeDuration)
        {
            // unscaledDeltaTime sayesinde oyun pause olsa bile efekt çalışır
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void OnEnable()
    {
        // Sahne her yüklendiğinde çalışacak fonksiyonu kaydet
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Bellek sızıntısını önlemek için kaydı sil
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sahne her açıldığında otomatik aydınlanma başlat
        StartCoroutine(ManualFadeIn());
    }
}