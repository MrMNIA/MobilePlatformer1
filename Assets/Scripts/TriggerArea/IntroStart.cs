using UnityEngine;

public class IntroStart : MonoBehaviour
{
    // EnemyAI script'ine sahip düşmanları buraya sürüklüyoruz
    public EnemyAI enemy;
    public float delay = 0f;

    public bool startBossMusic = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Sadece "Player" girdiğinde işlem yap
        if (other.CompareTag("Player"))
        {

            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }
            if(startBossMusic)
            SoundManager.Instance.ChangeMusicWithWait(SoundManager.Instance.bossMusic, delay);
            Invoke(nameof(StartIntro), delay);

        }
    }

    void StartIntro()
    {
        if (enemy != null)
        {
            enemy.ShowIntro();
        }
    }
}