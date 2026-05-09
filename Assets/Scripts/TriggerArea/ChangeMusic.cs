using UnityEngine;

public class ChangeMusic : MonoBehaviour
{

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

            SoundManager.Instance.musicSource.Stop();


        }
    }
}
