using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public enum CurrentPowerup { Speed, Attack, Shield, None }
public class PlayerPowerup : MonoBehaviour
{
    public GameObject player;
    public float powerupDurationTime = 5f;
    public Image powerupIcon;
    public Image powerupDurationIcon;
    public CurrentPowerup currentPowerup = CurrentPowerup.None;
    public SpriteRenderer playerPowerupIcon;
    public GameObject powerupObject;

    [Header("Icons")]
    public Sprite speedIcon;
    public Sprite attackIcon;
    public Sprite shieldIcon;
    private Sprite noneIcon;

    [Header("Audio")]
    public AudioClip speedSound;
    public AudioClip attackSound;
    public AudioClip shieldSound;


     private void Start()
     {
        noneIcon = powerupDurationIcon.GetComponent<Image>().sprite;
        if (DifficultyManager.Instance.selectedPowerup != DifficultyManager.CurrentPowerup.None)
        {
            TryGetPowerup((PowerupType)DifficultyManager.Instance.selectedPowerup);
        }
        playerPowerupIcon.sprite = null;
        powerupDurationTime += PlayerPrefs.GetInt("PowerupLevel", 0) * 0.5f; // Mağazadan alınan güçlendirme etkisi
    }
    public bool TryGetPowerup(PowerupType type)
    {
        if (currentPowerup != CurrentPowerup.None) return false;

        currentPowerup = (CurrentPowerup)type;
        switch (type)
        {
            case PowerupType.Speed:
                powerupIcon.sprite = speedIcon;
                break;
            case PowerupType.Attack:
                powerupIcon.sprite = attackIcon;
                break;
            case PowerupType.Shield:
                powerupIcon.sprite = shieldIcon;
                break;
        }
        powerupObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        return true;
    }

    public void TryUsePowerup()
    {
        if (currentPowerup != CurrentPowerup.None)
        {
            UsePowerup(currentPowerup, powerupDurationTime);
        }
    }

    private void Update() 
    {
        float parentDirection = player.transform.localScale.x < 0 ? -1f : 1f;

        Vector3 currentScale = playerPowerupIcon.transform.localScale;

        // İkonun X ölçeğini Player'ın yönüne göre sabitliyoruz.
        // Mathf.Abs kullanarak orijinal büyüklüğünü koruyup sadece yönünü veriyoruz.
        float targetX = Mathf.Abs(currentScale.x) * parentDirection;

        // Sadece değer farklıysa ata (titremeyi tamamen bitirir)
        if (!Mathf.Approximately(currentScale.x, targetX))
        {
            currentScale.x = targetX;
            playerPowerupIcon.transform.localScale = currentScale;
        }
    }
    public void UsePowerup(CurrentPowerup type, float duration)
    {
        switch (type)
        {
            case CurrentPowerup.Speed:
                player.GetComponent<PlayerMovement>().SpeedBoost(duration);
                SoundManager.Instance.PlaySound(speedSound);
                StartCoroutine(ShowDuration(duration));
                playerPowerupIcon.sprite = speedIcon; // Karakterin üstünde güçlendirme ikonunu göster
                break;
            case CurrentPowerup.Attack:
                player.GetComponent<PlayerMelee>().AttackBoost(duration);
                SoundManager.Instance.PlaySound(attackSound);
                StartCoroutine(ShowDuration(duration));
                playerPowerupIcon.sprite = attackIcon; // Karakterin üstünde güçlendirme ikonunu göster
                break;
            case CurrentPowerup.Shield:
                player.GetComponent<Health>().ShieldBoost(duration);
                SoundManager.Instance.PlaySound(shieldSound);
                StartCoroutine(ShowDuration(duration));
                playerPowerupIcon.sprite = shieldIcon; // Karakterin üstünde güçlendirme ikonunu göster
                break;
        }
    }

    private IEnumerator ShowDuration(float duration)
    {
        float timeLeft = duration;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            powerupDurationIcon.fillAmount = 1f - (timeLeft / duration);
            yield return null;
        }
        currentPowerup = CurrentPowerup.None;
        powerupIcon.sprite = noneIcon;
        powerupObject.transform.localScale = new Vector3(1f, 1f, 1); // İkonu eski boyutuna getir
        playerPowerupIcon.sprite = null; // Karakterin üstündeki güçlendirme ikonunu kaldır
        powerupDurationIcon.fillAmount = 0f;
    }
}
