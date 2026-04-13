using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

public class AttackJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    // Piksel hesab� yerine 0 ile 1 aras� oran kullanmak daha sa�l�kl�d�r.
    // 0.2f = Joystick'in %20'si kadar �ekilmi�se sald�r.
    [SerializeField] private float fireThreshold = 0.2f;

    private RectTransform joystickThumb;
    private RectTransform joystickBackground;
    [SerializeField] private Image cooldownImage;

    private bool ableToAttack = true;
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }

    public event Action OnJoystickReleased;

    private void Awake()
    {
        joystickBackground = GetComponent<RectTransform>();
        joystickThumb = transform.GetChild(1).GetComponent<RectTransform>();
        ResetValues();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ableToAttack) { return; }
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            // Pozisyonu joystick boyutuna oranla
            position = position / (joystickBackground.sizeDelta * 0.5f);

            // �emberin d���na ��k�yorsa 1'e sabitle (Normalize)
            if (position.magnitude > 1f)
            {
                position = position.normalized;
            }

            // Thumb'� hareket ettir ve de�erleri ata
            joystickThumb.anchoredPosition = position * (joystickBackground.sizeDelta * 0.5f);
            Horizontal = position.x;
            Vertical = position.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // �NEML� DE����KL�K:
        // Dokundu�un an, sanki s�r�klemi�sin gibi OnDrag'� tetikliyoruz.
        // B�ylece top direkt parma��n�n alt�na geliyor ve de�erler doluyor.
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ESK� MANTIK: (B�rak�lan Yer - Ba�lanan Yer) -> Kenara bas�nca 0 ��k�yordu.
        // YEN� MANTIK: Direkt (Horizontal, Vertical) b�y�kl���ne bak�yoruz.
        // ��nk� bu de�erler zaten Merkeze olan uzakl��� veriyor.

        Vector2 inputVector = new Vector2(Horizontal, Vertical);

        // E�er joystick merkezinden yeterince uzaksa (�rn: %20 �ekilmi�se)
        if (inputVector.magnitude >= fireThreshold)
        {
            if (OnJoystickReleased != null)
            {
                OnJoystickReleased.Invoke();
            }
        }

        ResetValues();
    }

    private void ResetValues()
    {
        joystickThumb.anchoredPosition = Vector2.zero;
        Horizontal = 0f;
        Vertical = 0f;
    }

    // Cooldown kodlar�n aynen kalabilir...
    public void CooldownCounter(float value)
    {
        StartCoroutine(UpdateCooldownImage(value));
    }

    private IEnumerator UpdateCooldownImage(float value)
    {
        float timer = value;
        cooldownImage.fillAmount = 1;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            cooldownImage.fillAmount = timer / value;
            yield return null;
        }
        cooldownImage.fillAmount = 0f;
    }

    public void ChangeAbleToAttack()
    {
        ableToAttack = !ableToAttack;
        if (!ableToAttack)
        {
            ResetValues(); //sald�r� engellendiinde joystick de sfrlanr
        }
    }
}