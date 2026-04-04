using UnityEngine;
using UnityEditor;
using UnityEngine.UI; // Normal Text bileşenleri için bu gerekli

public class ChangeFont : EditorWindow
{
    private Font yeniFont;

    [MenuItem("Tools/Change All Fonts")]
    public static void ShowWindow()
    {
        GetWindow<ChangeFont>("Font Changer");
    }

    void OnGUI()
    {
        GUILayout.Label("Klasik Text Font Ayarları", EditorStyles.boldLabel);

        // Font dosyasını (.ttf / .otf) buraya sürükleyeceksin
        yeniFont = (Font)EditorGUILayout.ObjectField("Yeni Font (CasterStreet)", yeniFont, typeof(Font), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Tüm Klasik Textleri Değiştir!"))
        {
            if (yeniFont == null)
            {
                Debug.LogError("Lütfen önce bir font dosyası sürükleyin!");
                return;
            }

            // Sahnede bulunan tüm standart Text bileşenlerini bulur
            Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
            int count = 0;

            foreach (Text text in allTexts)
            {
                // Sadece sahnede olanları etkilemek için kontrol
                if (text.gameObject.scene.name == null) continue;

                Undo.RecordObject(text, "Font Değişimi");
                text.font = yeniFont;
                EditorUtility.SetDirty(text);
                count++;
            }

            Debug.Log($"{count} adet klasik Text objesi başarıyla güncellendi.");
        }
    }
}