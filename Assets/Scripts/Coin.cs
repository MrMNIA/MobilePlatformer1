using UnityEngine;

public class Coin : MonoBehaviour
{
    public int baseValue = 1; // Temel coin değeri
    public AudioClip coinPickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Parayı ekle
            MoneyManager.Instance.AddCoins(baseValue);
            // Ses efektini çal
            SoundManager.Instance.PlaySound(coinPickupSound);

            // Coin'i yok et
            gameObject.SetActive(false);
        }
    }
}
