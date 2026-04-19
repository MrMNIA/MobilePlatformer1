using UnityEngine;

public enum PowerupType { Speed, Attack, Shield }
public class PowerPickup : MonoBehaviour
{
    public AudioClip pickupSound;
    public PowerupType type;
    [SerializeField] private PlayerPowerup powerup;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound(pickupSound);
            powerup.GetPowerup(type);
            gameObject.SetActive(false);
        }
    }
}
