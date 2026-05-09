using UnityEngine;

public class GorillaJumpTrigger : MonoBehaviour
{
    [Header("Zıplama Ayarları")]
    public float jumpForce = 18f;
    public float forwardBoost = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Goril'i bul ve onun metodunu çağır
        GorillaBossAI gorilla = other.GetComponent<GorillaBossAI>();

        if (gorilla != null)
        {
            // Fizik müdahalesi bitti, artık profesyonel bir metot çağrısı var!
            gorilla.ExecuteExternalJump(jumpForce, forwardBoost);
        }
    }
}