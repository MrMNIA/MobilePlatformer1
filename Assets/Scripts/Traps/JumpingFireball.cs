using UnityEngine;
using System.Collections;

public class JumpingFireball : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpHeight = 5f;        // Zıplama yüksekliği
    public float baseJumpDuration = 2f;  // Temel zıplama süresi (Easy için)
    public float baseIdleDuration = 1f;  // Zıplamalar arası temel bekleme süresi

    [Header("Start Delay")]
    public float startDelay = 0f;

    private Vector3 startingPosition;

    private void Start()
    {
        startingPosition = transform.position;

        if (startDelay > 0f)
            Invoke(nameof(StartJumping), startDelay);
        else
            StartJumping();
    }

    private void StartJumping()
    {
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            // 1. ZORLUK HESAPLAMASI
            // Çarpan 1.5 ise süreyi 1.5'e bölüyoruz (Böylece süre kısalıyor, hız artıyor)
            float multiplier = DifficultyManager.Instance.GetStatsMultiplier();
            float adjustedJumpDuration = baseJumpDuration / multiplier;
            float adjustedIdleDuration = baseIdleDuration / multiplier;

            // 2. ZIPLAMA (UP & DOWN)
            float timer = 0f;
            while (timer < adjustedJumpDuration)
            {
                timer += Time.deltaTime;
                float normalizedTime = timer / adjustedJumpDuration;

                // Parabolik formül: y = 4 * h * t * (1-t)
                float heightOffset = 4 * jumpHeight * normalizedTime * (1 - normalizedTime);
                transform.position = startingPosition + new Vector3(0, heightOffset, 0);

                if(timer >= adjustedJumpDuration / 2f)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 270);
                }

                yield return null;
            }

            // Pozisyonu tam sıfırla
            transform.position = startingPosition;

            // 3. IDLE (BEKLEME)
            yield return new WaitForSeconds(adjustedIdleDuration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = Application.isPlaying ? startingPosition : transform.position;
        Gizmos.DrawLine(start, start + Vector3.up * jumpHeight);
        Gizmos.DrawWireSphere(start + Vector3.up * jumpHeight, 0.2f);
    }
}