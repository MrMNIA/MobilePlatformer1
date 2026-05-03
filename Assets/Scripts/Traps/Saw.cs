using UnityEngine;
using System.Collections;

public class Saw : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float movementSpeed = 2f;
    public float maxDistance = 5f;
    public float idleDuration = 1f;

    [Range(0f, 360f)]
    public float movementAngle = 0f; // Müfettişten ayarlanabilen açı

    [Header("Görsel Ayarlar")]
    public float rotationSpeed = 500f;
    public float startDelay = 0f;

    private bool isIdling = true;
    private bool isMovingForward = true;
    private float currentDistance = 0f;
    private Vector3 startPosition;

    // Açıyı yön vektörüne çeviren yardımcı özellik
    private Vector3 MoveDirection
    {
        get
        {
            float rad = movementAngle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;
        }
    }

    private void Start()
    {
        startPosition = transform.position;

        if (startDelay > 0f)
        {
            StartCoroutine(Idle(startDelay));
        }
        else
        {
            isIdling = false;
        }
    }

    private void Update()
    {
        // 1. KENDİ ETRAFINDA DÖNME
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (isIdling) return;

        // 2. BELİRLENEN AÇIDA İLERLEME HAREKETİ
        float speedWithDifficulty = movementSpeed * DifficultyManager.Instance.GetStatsMultiplier();
        Vector3 direction = MoveDirection;

        if (isMovingForward)
        {
            transform.position += direction * speedWithDifficulty * Time.deltaTime;
            currentDistance += speedWithDifficulty * Time.deltaTime;

            if (currentDistance >= maxDistance)
            {
                StartCoroutine(Idle(idleDuration));
                isMovingForward = false;
            }
        }
        else
        {
            transform.position -= direction * speedWithDifficulty * Time.deltaTime;
            currentDistance -= speedWithDifficulty * Time.deltaTime;

            if (currentDistance <= -maxDistance)
            {
                StartCoroutine(Idle(idleDuration));
                isMovingForward = true;
            }
        }
    }

    private IEnumerator Idle(float duration)
    {
        isIdling = true;
        yield return new WaitForSeconds(duration);
        isIdling = false;
    }

    // EDİTÖRDE GÖRSELLEŞTİRME
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Oyun çalışmıyorsa o anki pozisyonu, çalışıyorsa başlangıç pozisyonunu baz al
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Vector3 direction = MoveDirection;

        // Belirlenen açıya göre uç noktaları hesapla
        Vector3 point1 = center + direction * maxDistance;
        Vector3 point2 = center - direction * maxDistance;

        // Menzil çizgisini çiz
        Gizmos.DrawLine(point1, point2);

        // Uç noktalara küçük küreler koy
        Gizmos.DrawWireSphere(point1, 0.3f);
        Gizmos.DrawWireSphere(point2, 0.3f);

        // Başlangıç noktasını belirtmek için küçük bir mavi küre (opsiyonel)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(center, 0.1f);
    }
}