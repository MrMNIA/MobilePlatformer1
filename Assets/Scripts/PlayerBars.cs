using UnityEngine;
using UnityEngine.UI;
public class PlayerBars : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Text healthText;

    float maxHealth;

    private void Start()
    {
        maxHealth = playerHealth.currentHealth;
        healthBar.fillAmount = 1;
    }
    private void Update()
    {
        float healthPercent = playerHealth.currentHealth / maxHealth;
        healthBar.fillAmount = healthPercent;
        healthText.text = healthPercent * 100 + "/" + maxHealth;
    }
}
