using UnityEngine;

public class ResetScript : MonoBehaviour
{
    void Awake()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=cyan><b>[SİSTEM]</b> Her şey sıfırlandı! Artık beni silebilirsin.</color>");
    }
}