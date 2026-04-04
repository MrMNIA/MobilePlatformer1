using UnityEngine;

public class Coin : MonoBehaviour
{
    public int baseValue = 1; // Temel coin değeri
    public AudioClip coinPickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Zorluk çarpanını al
            float multiplier = DifficultyManager.Instance.GetCoinMultiplier();
            int finalValue = Mathf.RoundToInt(baseValue * multiplier);

            // Parayı ekle
            MoneyManager.Instance.AddCoins(finalValue);
            // Ses efektini çal
            SoundManager.Instance.PlaySound(coinPickupSound);

            // Coin'i yok et
            gameObject.SetActive(false);
        }
    }
}
