using UnityEngine;
using UnityEngine.UI;

public class PlayerBars : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Text healthText;

    private float cachedMaxHealth;

    private void Start()
    {
        // Değeri bir kez alıyoruz
        cachedMaxHealth = playerHealth.maximumHealth;

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

        // Bölme işlemi (0'a bölme hatasına karşı küçük bir önlem)
        if (cachedMaxHealth > 0)
        {
            healthBar.fillAmount = current / cachedMaxHealth;
        }

        healthText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(cachedMaxHealth);
    }
}