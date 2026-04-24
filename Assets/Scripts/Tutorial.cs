using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public int index;
    public float duration = 2f; // Kamera odaklanma süresi
    public Transform target = null; // Kamera hedefi olarak kullanılacak Transform referansı

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ADIM: Herhangi bir şey çarptı mı?
        Debug.Log("Bir obje tetikleyiciye girdi: " + other.name);

        if (other.CompareTag("Player"))
        {
            CameraController.Instance.StartCinematicFocus(target, 0.25f, duration); // Kamera odaklanma süresi
        }
        transform.GetComponent<BoxCollider2D>().enabled = false; // Tutorial tetikleyicisini devre dışı bırak
    }
}