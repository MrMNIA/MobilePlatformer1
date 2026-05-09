using UnityEngine;

public class GorillaFallTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Gelen objenin Goril olup olmadığını kontrol et
        GorillaBossAI gorilla = other.GetComponent<GorillaBossAI>();

        if (gorilla != null)
        {
            // Goril'in ölmeden önceki o sonsuz düşüş animasyonunu başlat
            gorilla.TriggerFailingFall();

            Debug.Log("<color=magenta>[FINAL]:</color> Goril uçuruma düştü. Sahne tamamlanıyor...");

            // Eğer trigger'ın bir kez çalışması yeterliyse:
            // gameObject.SetActive(false);
        }
    }
}