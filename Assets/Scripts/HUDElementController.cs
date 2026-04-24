using UnityEngine;

public class HUDElementController : MonoBehaviour
{
    public string elementID;
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        ApplyMySettings();
    }

    public void ApplyMySettings()
    {
        // Manager'daki listede beni bul
        var myData = SettingsManager.Instance.currentSettings.elements.Find(x => x.elementID == elementID);

        if (myData != null)
        {
            rect.anchoredPosition = new Vector2(myData.posX, myData.posY);
            rect.localScale = Vector3.one * myData.scale;
        }
    }
}