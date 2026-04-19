using UnityEngine;
using UnityEngine.UI;

public class EnemyBars : MonoBehaviour
{
    [SerializeField] private Health enemyHealth;
    [SerializeField] private Transform enemy;
    [SerializeField] private Image healthBar;
    public RectTransform canvasRectTransform;
    private Vector3 initialScale;
    private void Start()
    {
        initialScale = canvasRectTransform.localScale;
        UpdateUI();
    }

    private void Update()
    {
        int leftOrRight = enemy.localScale.x > 0 ? 1 : -1;
        canvasRectTransform.localScale = new Vector3(initialScale.x * leftOrRight, initialScale.y, initialScale.z);
        UpdateUI();
        if (enemyHealth.isDead == true)
        {
            canvasRectTransform.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        float current = enemyHealth.currentHealth;

        // Bölme işlemi (0'a bölme hatasına karşı küçük bir önlem)
        if (enemyHealth.maximumHealth > 0)
        {
            healthBar.fillAmount = current / enemyHealth.maximumHealth;
        }
    }
}