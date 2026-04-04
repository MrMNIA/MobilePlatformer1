using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events; // UnityEventTools için gerekli

public class GameManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public string anaIsim = "Level";
    public int baslangicIndeksi = 1;

    [Header("Referans")]
    public MenuManager menuManager; // MenuManager scriptinin bağlı olduğu objeyi buraya sürükle

    [ContextMenu("Her Şeyi Ayarla (MenuManager Bağlantılı)")]
    public void KurulumuYap()
    {
        if (menuManager == null)
        {
            Debug.LogError("Lütfen MenuManager referansını Inspector'dan atayın!");
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform levelObj = transform.GetChild(i);
            int levelNo = i + baslangicIndeksi;

            // 1. Obje İsmi
            levelObj.name = anaIsim + levelNo;

            // 2. Text İçeriği
            Text uiYazisi = levelObj.GetComponentInChildren<Text>();
            if (uiYazisi != null)
            {
                uiYazisi.text = levelNo.ToString();
                EditorUtility.SetDirty(uiYazisi);
            }

            // 3. Buton ve MenuManager Metot Ataması
            Button btn = levelObj.GetComponent<Button>();
            if (btn == null) btn = levelObj.GetComponentInChildren<Button>();

            if (btn != null)
            {
                // Önceki listener'ları temizle (temiz bir kurulum için)
                while (btn.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                }

                // MenuManager.OpenDifficultyPopup(int) metodunu bağla
                UnityEventTools.AddIntPersistentListener(
                    btn.onClick,
                    menuManager.OpenDifficultyPopup,
                    levelNo
                );

                EditorUtility.SetDirty(btn);
            }
        }
        Debug.Log("İşlem Tamam: 10 Buton MenuManager'a bağlandı!");
    }
}