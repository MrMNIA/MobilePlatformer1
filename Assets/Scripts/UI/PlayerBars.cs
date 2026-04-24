using UnityEngine;
using UnityEngine.UI;

public class PlayerBars : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image criticalImage;
    [SerializeField] private Text healthText;
    [SerializeField] private Image energyBar;
    [SerializeField] private Text energyText;

    private float cachedMaxHealth;
    private float cachedMaxEnergy;

    private void Start()
    {
        // Değeri bir kez alıyoruz
        cachedMaxHealth = playerHealth.maximumHealth;
        cachedMaxEnergy = playerEnergy.maxEnergy;

        // Oyun açılır açılmaz barın doğru görünmesi için bir kez tetikliyoruz
        UpdateUI();
    }

    private void Update()
    {
        // Update içinde UpdateUI fonksiyonunu çağırıyoruz
        UpdateUI();
    }

    private void UpdateUI()
    {
        float current = playerHealth.currentHealth;
        float energyCurrent = playerEnergy.currentEnergy;

        // Bölme işlemi (0'a bölme hatasına karşı küçük bir önlem)
        if (cachedMaxHealth > 0)
        {
            healthBar.fillAmount = current / cachedMaxHealth;
        }

        healthText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(cachedMaxHealth);

        float healthPercent = current / cachedMaxHealth;

        if (healthPercent <= 0.5f)
        {
            Color c = criticalImage.color;
            c.a = 0.7f * (1-healthPercent);
            criticalImage.color = c;
        }
        else
        {
            Color c = criticalImage.color;
            c.a = 0f;
            criticalImage.color = c;
        }

        if (cachedMaxEnergy > 0)
        {
            energyBar.fillAmount = energyCurrent / cachedMaxEnergy;
        }
        energyText.text = Mathf.RoundToInt(energyCurrent) + " / " + Mathf.RoundToInt(cachedMaxEnergy);
    }
}