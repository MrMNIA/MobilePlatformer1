using UnityEngine;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    public enum TriggerType { Start, End }

    [Header("Tetikleyici Ayarları")]
    public TriggerType type;
    public float delay = 0.1f; // Daha hızlı tepki için düşürüldü

    [Header("Referanslar")]
    [SerializeField] private Health gorillaHealth;
    private GorillaBars uiManager;

    private void Awake()
    {
        uiManager = FindFirstObjectByType<GorillaBars>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == TriggerType.Start)
            {
                StartCoroutine(StartBossSequence());
            }
            else if (type == TriggerType.End)
            {
                StartCoroutine(EndBossSequence());
            }
        }
    }

    private IEnumerator StartBossSequence()
    {
        yield return new WaitForSeconds(delay);

        if (uiManager != null && gorillaHealth != null)
        {
            // Bu metod artık her şeyi normale döndürüp barı açar
            uiManager.InitializeBossBar(gorillaHealth);
            Debug.Log("<color=green>TETİKLEYİCİ:</color> Boss UI Açıldı.");
        }

        // Önemli: Eğer bu trigger'ın bir daha çalışmasını istemiyorsan burayı aktif edebilirsin
        // gameObject.SetActive(false); 
    }

    private IEnumerator EndBossSequence()
    {
        yield return new WaitForSeconds(delay);
        if (uiManager != null) uiManager.HideBar();
        Debug.Log("<color=gold>TETİKLEYİCİ:</color> Boss Yenildi.");
    }
}