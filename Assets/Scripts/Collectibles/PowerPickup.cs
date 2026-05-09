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
            bool gotPowerup = powerup.TryGetPowerup(type);
            if (gotPowerup)
            {
                SoundManager.Instance.PlaySound(pickupSound);
                gameObject.SetActive(false);
            }
        }
    }
}
