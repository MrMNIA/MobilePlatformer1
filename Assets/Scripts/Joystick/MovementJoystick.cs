using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MovementJoystick : MonoBehaviour
{
    // Karakter scriptinin okuyacağı değerler
    public float Horizontal { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsCrouching { get; private set; }

    private bool leftPressed;
    private bool rightPressed;

    private bool ableToMove = true;

    // SOL BUTON İÇİN (Event Trigger ile bağlanacak)
    public void SetLeftMove(bool isPressed)
    {
        if (!ableToMove) return;
        leftPressed = isPressed;
        UpdateHorizontal();
    }

    // SAĞ BUTON İÇİN (Event Trigger ile bağlanacak)
    public void SetRightMove(bool isPressed)
    {
        if (!ableToMove) return;
        rightPressed = isPressed;
        UpdateHorizontal();
    }

    // YATAY HAREKETİ HESAPLA
    private void UpdateHorizontal()
    {
        if (leftPressed && rightPressed) Horizontal = 0; // İkisine aynı anda basılıyorsa dur
        else if (leftPressed) Horizontal = -1f;
        else if (rightPressed) Horizontal = 1f;
        else Horizontal = 0f;
    }

    // ZIPLA BUTONU (Basıldığında bir kez tetiklenir veya basılı tutulur)
    public void SetJump(bool isPressed)
    {
        if (!ableToMove) return;
        IsJumping = isPressed;
    }

    // EĞİL BUTONU
    public void SetCrouch(bool isPressed)
    {
        if (!ableToMove) return;
        IsCrouching = isPressed;
    }

    // Hareket kısıtlama (Joystick scriptindeki fonksiyonun benzeri)
    public void ChangeAbleToMove(bool status)
    {
        ableToMove = status;
        if (!ableToMove)
        {
            ResetInputs();
        }
    }

    public IEnumerator TemporaryDisable(float duration)
    {
        ChangeAbleToMove(false);
        yield return new WaitForSeconds(duration);
        ChangeAbleToMove(true);
    }

    private void ResetInputs()
    {
        leftPressed = false;
        rightPressed = false;
        IsJumping = false;
        IsCrouching = false;
        Horizontal = 0f;
    }
}