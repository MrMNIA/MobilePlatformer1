using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndCutscene : MonoBehaviour
{
    public Text endText;
    public float fadeDuration = 1.5f;
    public float waitTime = 3.0f;
    void Start()
    {
        // Rengi al, alfa değerini 0 yap ve tekrar ata
        Color tempColor = endText.color;
        tempColor.a = 0f;
        endText.color = tempColor;

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1. Fade In
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // 2. Bekle
        yield return new WaitForSeconds(waitTime);

        // 3. Fade Out
        elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SceneManager.LoadScene(0    );
    }

    // Yardımcı fonksiyon: Rengi bozmadan sadece alfayı değiştirir
    void SetAlpha(float alpha)
    {
        Color c = endText.color;
        c.a = alpha;
        endText.color = c;
    }
}
