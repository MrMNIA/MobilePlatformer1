using UnityEngine;
using System.Collections;

public class SlamWaveManager : MonoBehaviour
{
    public GameObject slamDustPrefab;
    public LayerMask combinedLayer;
    public LayerMask wallLayer;
    public float stepDistance = 1.2f;
    public float waveDelay = 0.05f;
    public int maxSteps = 20;

    public void StartWave(Vector3 startPos)
    {
        // 1. MERKEZ NOKTASINA (GORİLİN TAM ALTINA) TOZ BULUTU KOY
        SpawnInitialDust(startPos);

        // 2. DALGALARI SAĞA VE SOLA BAŞLAT
        StartCoroutine(SpawnDynamicWave(1f, startPos));  // Sağ
        StartCoroutine(SpawnDynamicWave(-1f, startPos)); // Sol

        Destroy(gameObject, 3f);
    }

    private void SpawnInitialDust(Vector3 origin)
    {
        // Gorilin tam altında zemin kontrolü yap
        float fixedRayStartY = origin.y + 1.0f;
        Vector3 rayOrigin = new Vector3(origin.x, fixedRayStartY, 0);
        RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, 5f, combinedLayer);

        if (groundHit.collider != null)
        {
            Vector3 spawnPos = new Vector3(groundHit.point.x, groundHit.point.y + 0.1f, 0);
            Instantiate(slamDustPrefab, spawnPos, Quaternion.identity);
        }
    }

    IEnumerator SpawnDynamicWave(float direction, Vector3 origin)
    {
        Vector3 currentPos = origin;
        float fixedRayStartY = origin.y + 1.0f;

        for (int i = 0; i < maxSteps; i++)
        {
            yield return new WaitForSeconds(waveDelay);

            // İlk adımda zaten merkezi doldurduğumuz için öteleme yaparak devam ediyoruz
            float nextX = currentPos.x + (direction * stepDistance);
            Vector3 rayOrigin = new Vector3(nextX, fixedRayStartY, 0);

            RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, 5f, combinedLayer);
            RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.right * direction, stepDistance, wallLayer);

            if (groundHit.collider != null && wallHit.collider == null)
            {
                Vector3 spawnPos = new Vector3(groundHit.point.x, groundHit.point.y + 0.1f, 0);
                Instantiate(slamDustPrefab, spawnPos, Quaternion.identity);
                currentPos = spawnPos;
            }
            else
            {
                break;
            }
        }
    }
}