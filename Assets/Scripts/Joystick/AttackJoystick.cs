using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;

public class AttackJoystick : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image cooldownImage;

    private bool ableToAttack = true;

    // JoystickReleased yerine daha uygun bir isim
    public event Action OnAttackPressed;

    private void Awake()
    {
        // Başlangıçta cooldown resmini sıfırla
        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;
    }

    public void TryToAttack()
    {
        Debug.Log("TryToAttack");
        if (ableToAttack)
        {
            Debug.Log("Attack");
            Attack();
        }
    }
    private void Attack()
    {
        // Event'i dinleyen (Karakter scripti vb.) varsa tetikle
        OnAttackPressed?.Invoke();
        Debug.Log("AttackInvoked");

        // Örnek: Butona basıldığında otomatik cooldown başlatmak istersen burayı kullanabilirsin.
        // Eğer cooldown süresini karakter scriptinden yönetiyorsan burayı boş bırakabilirsin.
    }

    // Karakter scriptinden bu fonksiyon çağrılarak cooldown başlatılır
    public void StartCooldown(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(UpdateCooldownImage(duration));
        }
    }

    private IEnumerator UpdateCooldownImage(float duration)
    {
        ableToAttack = false;
        float timer = duration;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                cooldownImage.fillAmount = timer / duration;
                yield return null;
            }
            cooldownImage.fillAmount = 0f;
        }

        ableToAttack = true;
    }

    // Dışarıdan saldırı yeteneğini tamamen açıp kapatmak için (Örn: Menü açıldığında)
    public void ChangeAbleToAttack(bool status)
    {
        ableToAttack = status;
    }
}