using UnityEngine;

public class WinZone : MonoBehaviour
{
    private bool isWon = false; // Birden fazla tetiklenmeyi önlemek için

    private void OnTriggerEnter2D(Collider2D other)
    {

        // 1. Gelen obje Player mı?
        // 2. Daha önce kazanıldı mı?
        if (other.CompareTag("Player") && !isWon)
        {
            isWon = true;
            WinGame();
        }
    }

    void WinGame()
    {
        // UIManager üzerinden Win ekranını açıyoruz
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWinScreen();
        }
    }
}