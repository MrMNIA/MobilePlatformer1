using UnityEngine;
using System.Collections;

public class Saw : MonoBehaviour
{
    public float movementSpeed = 2f;
    public float maxDistance = 5f;
    public float idleDuration = 1f;
    public float rotationSpeed = 500f; // Dönme hızı
    public float startDelay = 0f; // Başlangıçta bekleme süresi

    private bool isIdling = true;
    private bool isMovingRight = true;
    private float currentDistance = 0f;
    private Vector3 startPosition;

    private void Start()
    {
        // Başlangıç pozisyonunu gizmos için kaydediyoruz
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
        // 1. DÖNME HAREKETİ (Kod ile)
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (isIdling) return;

        // 2. İLERLEME HAREKETİ
        if (isMovingRight)
        {
            transform.position += Vector3.right * movementSpeed * DifficultyManager.Instance.GetStatsMultiplier() * Time.deltaTime;
            currentDistance += movementSpeed * DifficultyManager.Instance.GetStatsMultiplier() * Time.deltaTime;

            if (currentDistance >= maxDistance)
            {
                StartCoroutine(Idle(idleDuration));
                isMovingRight = false;
            }
        }
        else
        {
            transform.position += Vector3.left * movementSpeed * DifficultyManager.Instance.GetStatsMultiplier() * Time.deltaTime;
            currentDistance -= movementSpeed * DifficultyManager.Instance.GetStatsMultiplier() * Time.deltaTime;

            if (currentDistance <= -maxDistance)
            {
                StartCoroutine(Idle(idleDuration));
                isMovingRight = true;
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

        Vector3 leftPoint = center + Vector3.left * maxDistance;
        Vector3 rightPoint = center + Vector3.right * maxDistance;

        // Menzil çizgisini çiz
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Uç noktalara küçük küreler koy
        Gizmos.DrawWireSphere(leftPoint, 0.3f);
        Gizmos.DrawWireSphere(rightPoint, 0.3f);
    }
}