using UnityEngine;

public class StartLavaRising : MonoBehaviour
{
    public float delay = 2f; // Her bir hedefe odaklanma süresi

    public RisingLava risingLava;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tetikleyiciyi kapat ki tekrar tekrar çalışmasın
            if (TryGetComponent<BoxCollider2D>(out BoxCollider2D col))
            {
                col.enabled = false;
            }

            Invoke(nameof(Basla), delay);

        }
    }

    void Basla()
    {
        risingLava.LavYukselt();
    }

}