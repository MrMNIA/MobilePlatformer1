using UnityEngine;

public class EnemyAwake : MonoBehaviour
{
    // EnemyAI script'ine sahip düşmanları buraya sürüklüyoruz
    public EnemyAI[] enemies;
    public float delay = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Sadece "Player" girdiğinde işlem yap
        if (other.CompareTag("Player"))
        {         
            
            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }

            Invoke(nameof(AwakeEnemies), delay);
        }
    }

    void AwakeEnemies()
    {
        if (enemies != null)
        {
            foreach (EnemyAI enemy in enemies)
            {
                // Düşman silinmiş olabilir, kontrol et
                if (enemy != null)
                {
                    // ÖNEMLİ: İsmi Awake değil, senin yazdığın metot olmalı
                    enemy.WakeUp();
                }
            }
        }
    }
}