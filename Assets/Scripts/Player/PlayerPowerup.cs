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

    [Header("Icons")]
    public Sprite speedIcon;
    public Sprite attackIcon;
    public Sprite shieldIcon;
    private Sprite noneIcon;

    [Header("Audio")]
    public AudioClip speedSound;
    public AudioClip attackSound;
    public AudioClip shieldSound;


     private void Start() {
        noneIcon = powerupDurationIcon.GetComponent<Image>().sprite;
        powerupDurationTime += PlayerPrefs.GetInt("PowerupLevel", 0) * 0.5f; // Mağazadan alınan güçlendirme etkisi
    }
    public void GetPowerup(PowerupType type)
    {
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
        powerupIcon.GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1); // İkonu büyüt
    }

    public void TryUsePowerup()
    {
        if (currentPowerup != CurrentPowerup.None)
        {
            UsePowerup(currentPowerup, powerupDurationTime);
        }
    }
    public void UsePowerup(CurrentPowerup type, float duration)
    {
        switch (type)
        {
            case CurrentPowerup.Speed:
                player.GetComponent<PlayerMovement>().SpeedBoost(duration);
                StartCoroutine(ShowDuration(duration));
                break;
            case CurrentPowerup.Attack:
                player.GetComponent<PlayerMelee>().AttackBoost(duration);
                StartCoroutine(ShowDuration(duration));
                break;
            case CurrentPowerup.Shield:
                player.GetComponent<Health>().ShieldBoost(duration);
                StartCoroutine(ShowDuration(duration));
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
        powerupIcon.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1); // İkonu eski boyutuna getir
        powerupDurationIcon.fillAmount = 0f;
    }
}
