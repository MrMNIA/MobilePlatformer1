using System.Collections.Generic;
using UnityEngine;
using System.IO;

// 1. Her bir butonun verisi
[System.Serializable]
public class HUDElementData
{
    public string elementID;
    public float posX;
    public float posY;
    public float scale;

    public HUDElementData() { }

    public HUDElementData(string id, float x, float y, float s)
    {
        this.elementID = id;
        this.posX = x;
        this.posY = y;
        this.scale = s;
    }
}

// 2. TÜM butonları içeren liste (Kayıt dosyası için şart)
[System.Serializable]
public class HUDLayoutContainer
{
    public List<HUDElementData> elements = new List<HUDElementData>();
}

// 3. Ana Kontrolcü
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public HUDLayoutContainer currentSettings = new HUDLayoutContainer();
    public List<HUDElementData> defaultLayout = new List<HUDElementData>();

    private string SavePath => Application.persistentDataPath + "/hud_layout.json";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        LoadLayout();
    }

    [ContextMenu("Sahnedeki Dizilimi Default Yap")]
    public void CaptureCurrentAsDefault()
    {
        defaultLayout.Clear();
        // Sahnedeki HUDElementController scriptine sahip her şeyi bulur
        HUDElementController[] allElements = FindObjectsOfType<HUDElementController>();

        foreach (var element in allElements)
        {
            RectTransform rt = element.GetComponent<RectTransform>();
            defaultLayout.Add(new HUDElementData(
                element.elementID,
                rt.anchoredPosition.x,
                rt.anchoredPosition.y,
                element.transform.localScale.x
            ));
        }
        Debug.Log("Sistem: " + defaultLayout.Count + " adet buton varsayılan olarak kaydedildi.");
    }

    public void SaveLayout()
    {
        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Ayarlar Dosyaya Yazıldı.");
    }

    public void LoadLayout()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            currentSettings = JsonUtility.FromJson<HUDLayoutContainer>(json);
        }
        else
        {
            // Direkt eşitlemek yerine kopyalayarak aktar
            currentSettings.elements.Clear();
            foreach (var d in defaultLayout)
            {
                currentSettings.elements.Add(new HUDElementData(d.elementID, d.posX, d.posY, d.scale));
            }
        }
    }
    public void ResetToDefault()
    {
        currentSettings.elements.Clear();

        foreach (var d in defaultLayout)
        {
            // Yeni bir HUDElementData nesnesi oluşturarak bağı koparıyoruz
            currentSettings.elements.Add(new HUDElementData(d.elementID, d.posX, d.posY, d.scale));
        }

        HUDElementController[] allElements = FindObjectsOfType<HUDElementController>();
        foreach (var element in allElements)
        {
            element.ApplyMySettings();
        }

        SaveLayout();
        Debug.Log("Sistem: Varsayılan değerler kopyalandı ve üzerine yazıldı.");
    }
}