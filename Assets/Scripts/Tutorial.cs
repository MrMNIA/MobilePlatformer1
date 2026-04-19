using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public int index;
    public Transform target = null; // Kamera hedefi olarak kullanılacak Transform referansı

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ADIM: Herhangi bir şey çarptı mı?
        Debug.Log("Bir obje tetikleyiciye girdi: " + other.name);

        if (other.CompareTag("Player"))
        {
                UIManager.instance.ShowTutorial(index, target);
        }
        transform.GetComponent<BoxCollider2D>().enabled = false; // Tutorial tetikleyicisini devre dışı bırak
    }
}