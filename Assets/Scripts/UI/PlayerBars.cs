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

    // cachedMax değişkenlerini sildik veya sadece referans amaçlı bıraktık
    // Çünkü değerler oyun başında Health scripti tarafından değiştiriliyor.

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Health ve Energy scriptlerinden GÜNCEL maximum değerleri alıyoruz
        float maxH = playerHealth.maximumHealth;
        float curH = playerHealth.currentHealth;

        float maxE = playerEnergy.maxEnergy;
        float curE = playerEnergy.currentEnergy;

        // Can Barı Güncelleme
        if (maxH > 0)
        {
            healthBar.fillAmount = curH / maxH;
            healthText.text = Mathf.RoundToInt(curH) + " / " + Mathf.RoundToInt(maxH);

            float healthPercent = curH / maxH;

            // Kritik Sağlık Görseli (Ekran kenarı kızarması vb.)
            if (healthPercent <= 0.5f)
            {
                Color c = criticalImage.color;
                c.a = 0.7f * (1 - healthPercent);
                criticalImage.color = c;
            }
            else
            {
                Color c = criticalImage.color;
                c.a = 0f;
                criticalImage.color = c;
            }
        }

        // Enerji Barı Güncelleme
        if (maxE > 0)
        {
            energyBar.fillAmount = curE / maxE;
            energyText.text = Mathf.RoundToInt(curE) + " / " + Mathf.RoundToInt(maxE);
        }
    }
}