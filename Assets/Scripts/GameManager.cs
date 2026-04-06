using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using System.Collections; // Coroutine için gerekli

public class GameManager : MonoBehaviour
{
    [Header("Otomasyon Ayarları")]
    public string anaIsim = "Level";
    public int baslangicIndeksi = 1;
    public string kilitResmi = "lockIcon";

    private int currentChapter = 0; // Hangi chapter'da olduğumuzu takip etmek için

    [Header("Referanslar")]
    public MenuManager menuManager;
    public RectTransform levelsContainer; // Chapter'ların içinde olduğu ana klasör (Levels objesi)
    public CanvasGroup levelsCanvasGroup; // Tıklamaları kapatmak için ana objeye Canvas Group ekle
    public Canvas anaCanvas; // Buraya ana Canvas objesini sürükle

    public Image[] chapterImages = new Image[2]; // Chapter görselleri (3 chapter olduğunu varsayıyoruz)
    public Text chapterTitle; // Chapter başlıklarını değiştirmek için

    [Header("Geçiş Ayarları")]
    public float gecisSuresi = 0.5f;
    private bool isMoving = false;

    // --- MEVCUT OTOMASYON METODUN ---
    [ContextMenu("Her Şeyi Ayarla (Chapter Destekli)")]
    public void KurulumuYap()
    {
        if (menuManager == null)
        {
            Debug.LogError("Lütfen MenuManager referansını Inspector'dan atayın!");
            return;
        }

        int currentLevelNo = baslangicIndeksi;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform chapter = transform.GetChild(i);
            for (int j = 0; j < chapter.childCount; j++)
            {
                Transform levelObj = chapter.GetChild(j);
                levelObj.name = anaIsim + currentLevelNo;

                Text uiYazisi = levelObj.GetComponentInChildren<Text>();
                if (uiYazisi != null)
                {
                    uiYazisi.text = currentLevelNo.ToString();
                    EditorUtility.SetDirty(uiYazisi);
                }

                Button btn = levelObj.GetComponent<Button>();
                if (btn == null) btn = levelObj.GetComponentInChildren<Button>();

                if (btn != null)
                {
                    while (btn.onClick.GetPersistentEventCount() > 0)
                    {
                        UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                    }

                    UnityEventTools.AddIntPersistentListener(
                        btn.onClick,
                        menuManager.OpenDifficultyPopup,
                        currentLevelNo
                    );
                    EditorUtility.SetDirty(btn);
                }
                currentLevelNo++;
            }
        }
        Debug.Log("Butonlar ve hiyerarşi güncellendi!");
    }

    // --- YENİ EKLENEN GEÇİŞ METODU ---

    // Butona bunu ata: int parametresine sağa gitmek için 1, sola gitmek için -1 yaz.
    public void ChapterDegistir(int yon)
    {
        if (isMoving) return;
        if ((yon == 1 && currentChapter >= transform.childCount - 1) || (yon == -1 && currentChapter <= 0))
            return; // Sınırları kontrol et

        // 1280 yerine RectTransform'un genişliğini kullanıyoruz.
        // Bu sayede ekran çözünürlüğü ne olursa olsun tam 1 ekran boyu kayar.
        float kaymaMiktari = anaCanvas.GetComponent<RectTransform>().rect.width;

        Vector2 hedefPos = levelsContainer.anchoredPosition + new Vector2(yon * -kaymaMiktari, 0);

        StartCoroutine(SmoothMove(hedefPos));
        StartCoroutine(ChangeBackground(currentChapter, currentChapter + yon));
        currentChapter += yon;
        chapterTitle.text = "Chapter " + (currentChapter + 1); // Başlık güncellemesi (1 tabanlı gösterim)
    }

    IEnumerator ChangeBackground(int oldIndex, int newIndex)
    {
        float elapsed = 0f;

        // Başlangıç renklerini alalım
        Color oldStartColor = chapterImages[oldIndex].color;
        Color newStartColor = chapterImages[newIndex].color;

        while (elapsed < gecisSuresi)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / gecisSuresi; // 0 ile 1 arasında bir değer

            // Eski resmi fade out (karart), yeni resmi fade in (parlat) yap
            // Mathf.Lerp kullanarak çok daha pürüzsüz bir geçiş sağlarız
            chapterImages[oldIndex].color = new Color(oldStartColor.r, oldStartColor.g, oldStartColor.b, Mathf.Lerp(1f, 0f, t));
            chapterImages[newIndex].color = new Color(newStartColor.r, newStartColor.g, newStartColor.b, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        // Değerleri tam olarak 0 ve 1'e eşitleyelim (Garantiye almak için)
        chapterImages[oldIndex].color = new Color(oldStartColor.r, oldStartColor.g, oldStartColor.b, 0f);
        chapterImages[newIndex].color = new Color(newStartColor.r, newStartColor.g, newStartColor.b, 1f);
    }
    IEnumerator SmoothMove(Vector2 hedef)
    {
        isMoving = true;

        // Tıklamaları kapat
        if (levelsCanvasGroup != null) levelsCanvasGroup.interactable = false;

        Vector2 baslangicPos = levelsContainer.anchoredPosition;
        float gecenSure = 0;

        while (gecenSure < gecisSuresi)
        {
            gecenSure += Time.deltaTime;
            float t = gecenSure / gecisSuresi;

            // Yumuşak geçiş için (SmoothStep)
            t = t * t * (3f - 2f * t);

            levelsContainer.anchoredPosition = Vector2.Lerp(baslangicPos, hedef, t);
            yield return null;
        }

        levelsContainer.anchoredPosition = hedef;

        // Tıklamaları tekrar aç
        if (levelsCanvasGroup != null) levelsCanvasGroup.interactable = true;

        isMoving = false;
    }

    private void Start()
    {
        ButonKilitleriniGuncelle();
        chapterTitle.text = "Chapter " + (currentChapter + 1); // Başlık güncellemesi (1 tabanlı gösterim)
    }

    public void ButonKilitleriniGuncelle()
    {
        int currentLevelNo = baslangicIndeksi;

        for (int i = 0; i < transform.childCount; i++) // Chapterlar
        {
            Transform chapter = transform.GetChild(i);
            for (int j = 0; j < chapter.childCount; j++) // Levellar
            {
                Transform levelObj = chapter.GetChild(j);
                Button btn = levelObj.GetComponent<Button>();
                if (btn == null) btn = levelObj.GetComponentInChildren<Button>();

                if (btn != null)
                {
                    // Kilit Mantığı: 
                    // 1. Level her zaman açık olmalı.
                    // Diğerleri için bir önceki level tamamlanmış mı diye bak (Level(N-1)_Completed)
                    bool kilidiAcik = false;

                    if (currentLevelNo == baslangicIndeksi)
                    {
                        kilidiAcik = true; // İlk level hep açık
                    }
                    else
                    {
                        string oncekiLevelKey = "Level" + (currentLevelNo - 1) + "_Completed";
                        // PlayerPrefs'te 1 ise tamamlanmış, 0 ise tamamlanmamıştır
                        if (PlayerPrefs.GetInt(oncekiLevelKey, 0) == 1)
                        {
                            kilidiAcik = true;
                        }
                    }

                    // Butonu aktif/pasif yap
                    btn.interactable = kilidiAcik;

                    // Kilit görselini bul ve göster/gizle
                    Transform lockImg = levelObj.Find(kilitResmi);
                    if (lockImg != null)
                    {
                        lockImg.gameObject.SetActive(!kilidiAcik); // Kilidi açıksa resmi gizle
                    }
                }
                currentLevelNo++;
            }
        }
    }
}