using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GorillaBars : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image bossIcon;
    [SerializeField] private Text healthText;

    [Header("Görsel Ayarlar")]
    [SerializeField] private Sprite normalIcon;
    [SerializeField] private Sprite enrageIcon;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enrageGrey = Color.gray;

    private Health targetHealth;
    private GorillaBossAI gorilRef;
    private bool isEnragedSequence = false;

    private void Awake()
    {
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
    }

    // Trigger (Start) bunu çağırdığında barı görünür yapar
    public void InitializeBossBar(Health healthScript)
    {
        targetHealth = healthScript;
        gorilRef = healthScript.GetComponent<GorillaBossAI>();

        if (bossUIPanel != null) bossUIPanel.SetActive(true);

        // Eğer goril öfkeliyse, açıldığında otomatik olarak öfkeli görselleriyle başlasın
        if (gorilRef != null && gorilRef.isBossRaged)
        {
            isEnragedSequence = true; // RUN!!! yazısının kalıcı olmasını sağlar
            ApplyEnragedVisuals();
        }
        else
        {
            isEnragedSequence = false;
            if (healthBarFill != null) healthBarFill.color = normalColor;
            if (bossIcon != null && normalIcon != null) bossIcon.sprite = normalIcon;
        }

        UpdateUI();
    }

    // Trigger (End) bunu çağırdığında barı gizler
    public void HideBar()
    {
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
    }

    // Goril öfkelendiğinde sadece görselleri hazırlar, KAPANMAZ.
    public void ActivateEnrageUI()
    {
        isEnragedSequence = true;
        ApplyEnragedVisuals();
        Debug.Log("<color=red>UI:</color> Görseller öfke moduna hazırlandı.");
    }

    private void ApplyEnragedVisuals()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = enrageGrey;
            // Öfke anında canı fullendiği için görseli de fulleyelim
            healthBarFill.fillAmount = 1f;
        }
        if (healthText != null) healthText.text = "RUN!!!";
        if (bossIcon != null && enrageIcon != null) bossIcon.sprite = enrageIcon;
    }

    private void Update()
    {
        if (targetHealth == null || !bossUIPanel.activeSelf) return;

        UpdateUI();

        // Ölüm kontrolü
        if (targetHealth.currentHealth <= 0)
        {
            HideBar();
        }
    }

    // EKSİK OLAN METOT BURADA:
    private void UpdateUI()
    {
        if (targetHealth == null) return;

        // Bar doluluğunu her zaman güncelle
        float fillRatio = targetHealth.currentHealth / targetHealth.maximumHealth;
        if (healthBarFill != null) healthBarFill.fillAmount = Mathf.Clamp01(fillRatio);

        // Yazı kontrolü
        if (isEnragedSequence)
        {
            if (healthText != null) healthText.text = "RUN!!!";
        }
        else
        {
            if (healthText != null) healthText.text = Mathf.CeilToInt(targetHealth.currentHealth).ToString();
        }
    }
}