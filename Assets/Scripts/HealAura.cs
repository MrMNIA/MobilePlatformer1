using UnityEngine;
using System.Collections;

public class HealEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    public float fadeDuration = 0.5f; // Ne kadar sürede belirip kaybolacak?
    public float stayDuration = 0.2f; // Tam görünür halde ne kadar bekleyecek?

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Başlangıçta tamamen şeffaf yap (0 alpha)
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0;
            spriteRenderer.color = c;
        }
    }

    private void Start()
    {
        // Efekt yaratıldığı an süreci başlat
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // 1. Belirme (0'dan 1'e)
        yield return StartCoroutine(Fade(0, 1));

        // 2. Kısa bir süre bekle
        yield return new WaitForSeconds(stayDuration);

        // 3. Kaybolma (1'den 0'a)
        yield return StartCoroutine(Fade(1, 0));

        // 4. Objeyi yok et (Havuza geri göndermiyoruz, efekt olduğu için direkt silebiliriz)
        Destroy(gameObject);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0;
        Color c = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Alpha değerini zamana yayarak değiştir
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            spriteRenderer.color = c;
            yield return null;
        }

        // Tam değeri eşitle
        c.a = endAlpha;
        spriteRenderer.color = c;
    }
}