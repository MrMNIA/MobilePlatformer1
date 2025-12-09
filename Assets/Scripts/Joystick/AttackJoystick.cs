using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

public class AttackJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    // Piksel hesabý yerine 0 ile 1 arasý oran kullanmak daha saðlýklýdýr.
    // 0.2f = Joystick'in %20'si kadar çekilmiþse saldýr.
    [SerializeField] private float fireThreshold = 0.2f;

    private RectTransform joystickThumb;
    private RectTransform joystickBackground;
    [SerializeField] private Image cooldownImage;

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
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            // Pozisyonu joystick boyutuna oranla
            position = position / (joystickBackground.sizeDelta * 0.5f);

            // Çemberin dýþýna çýkýyorsa 1'e sabitle (Normalize)
            if (position.magnitude > 1f)
            {
                position = position.normalized;
            }

            // Thumb'ý hareket ettir ve deðerleri ata
            joystickThumb.anchoredPosition = position * (joystickBackground.sizeDelta * 0.5f);
            Horizontal = position.x;
            Vertical = position.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ÖNEMLÝ DEÐÝÞÝKLÝK:
        // Dokunduðun an, sanki sürüklemiþsin gibi OnDrag'ý tetikliyoruz.
        // Böylece top direkt parmaðýnýn altýna geliyor ve deðerler doluyor.
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ESKÝ MANTIK: (Býrakýlan Yer - Baþlanan Yer) -> Kenara basýnca 0 çýkýyordu.
        // YENÝ MANTIK: Direkt (Horizontal, Vertical) büyüklüðüne bakýyoruz.
        // Çünkü bu deðerler zaten Merkeze olan uzaklýðý veriyor.

        Vector2 inputVector = new Vector2(Horizontal, Vertical);

        // Eðer joystick merkezinden yeterince uzaksa (Örn: %20 çekilmiþse)
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

    // Cooldown kodlarýn aynen kalabilir...
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
}