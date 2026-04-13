using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private int currentChapter = 0;

    [Header("Referanslar")]
    public MenuManager menuManager;
    public RectTransform levelsContainer; // Chapter'ların içinde olduğu ebeveyn obje
    public CanvasGroup levelsCanvasGroup;
    public Canvas anaCanvas;

    public Image[] chapterImages; // Arkaplan görselleri
    public Text chapterTitle; // "Chapter 1" yazısı

    [Header("Geçiş Ayarları")]
    public float gecisSuresi = 0.5f;
    public string kilitGorselAdi = "LockIcon"; // Kilit görselinin objesinin adı
    private bool isMoving = false;

    private void Start()
    {
        // Başlangıçta butonları ve başlığı güncelle
        ButonKilitleriniGuncelle();
        UpdateTitle();
    }

    public void ChapterDegistir(int yon)
    {
        if (isMoving) return;

        int maxChapter = levelsContainer.childCount;
        int nextChapter = currentChapter + yon;

        if (nextChapter < 0 || nextChapter >= maxChapter) return;

        // ESKİ YÖNTEM: Canvas genişliği kadar kaydır (Hata payı yüksek)
        // float kaymaMiktari = anaCanvas.GetComponent<RectTransform>().rect.width;
        // Vector2 hedefPos = levelsContainer.anchoredPosition + new Vector2(yon * -kaymaMiktari, 0);

        // YENİ YÖNTEM: Hedef child objesinin yerel pozisyonuna göre merkeze odaklan
        // levelsContainer içindeki her bir Chapter objesinin (0,0) noktasını referans alıyoruz.
        float hedefX = -levelsContainer.GetChild(nextChapter).localPosition.x;
        Vector2 hedefPos = new Vector2(hedefX, levelsContainer.anchoredPosition.y);

        StartCoroutine(SmoothMove(hedefPos));

        if (chapterImages != null && chapterImages.Length > nextChapter)
        {
            StartCoroutine(ChangeBackground(currentChapter, nextChapter));
        }

        currentChapter = nextChapter;
        UpdateTitle();
    }

    void UpdateTitle()
    {
        if (chapterTitle != null)
            chapterTitle.text = "Chapter " + (currentChapter + 1);
    }

    IEnumerator ChangeBackground(int oldIndex, int newIndex)
    {
        float elapsed = 0f;
        Color oldStartColor = chapterImages[oldIndex].color;
        Color newStartColor = chapterImages[newIndex].color;

        while (elapsed < gecisSuresi)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / gecisSuresi;

            chapterImages[oldIndex].color = new Color(oldStartColor.r, oldStartColor.g, oldStartColor.b, Mathf.Lerp(1f, 0f, t));
            chapterImages[newIndex].color = new Color(newStartColor.r, newStartColor.g, newStartColor.b, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        chapterImages[oldIndex].color = new Color(oldStartColor.r, oldStartColor.g, oldStartColor.b, 0f);
        chapterImages[newIndex].color = new Color(newStartColor.r, newStartColor.g, newStartColor.b, 1f);
    }

    IEnumerator SmoothMove(Vector2 hedef)
    {
        isMoving = true;
        if (levelsCanvasGroup != null) levelsCanvasGroup.interactable = false;

        Vector2 baslangicPos = levelsContainer.anchoredPosition;
        float gecenSure = 0;

        while (gecenSure < gecisSuresi)
        {
            gecenSure += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, gecenSure / gecisSuresi);

            levelsContainer.anchoredPosition = Vector2.Lerp(baslangicPos, hedef, t);
            yield return null;
        }

        levelsContainer.anchoredPosition = hedef;
        if (levelsCanvasGroup != null) levelsCanvasGroup.interactable = true;
        isMoving = false;
    }

    public void ButonKilitleriniGuncelle()
    {
        int levelCounter = 1; // Level 1, Level 2 şeklinde gitmesi için

        // levelsContainer içindeki Chapter'ları dön
        foreach (Transform chapter in levelsContainer)
        {
            // Chapter içindeki Level butonlarını dön
            foreach (Transform levelObj in chapter)
            {
                Button btn = levelObj.GetComponent<Button>();
                if (btn == null) btn = levelObj.GetComponentInChildren<Button>();

                if (btn != null)
                {
                    bool kilidiAcik = false;

                    if (levelCounter == 1)
                    {
                        kilidiAcik = true; // İlk level daima açık
                    }
                    else
                    {
                        // Önceki level'ın tamamlanma durumuna bak
                        string oncekiLevelKey = "Level" + (levelCounter - 1) + "_Completed";
                        if (PlayerPrefs.GetInt(oncekiLevelKey, 0) == 1)
                        {
                            kilidiAcik = true;
                        }
                    }

                    btn.interactable = kilidiAcik;

                    // Kilit görselini bul ve aktifliğini ayarla
                    Transform lockImg = levelObj.Find(kilitGorselAdi);
                    if (lockImg != null)
                    {
                        lockImg.gameObject.SetActive(!kilidiAcik);
                    }
                }
                levelCounter++;
            }
        }
    }
}