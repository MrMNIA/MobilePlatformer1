using UnityEngine;
using System.Collections;

public class DoorSequenceDeactivator : MonoBehaviour
{
    public GameObject[] doors;
    public float initialDelay = 1.0f;
    public float stepInterval = 0.8f;

    private void OnEnable() => GorillaBossAI.OnGorillaEnraged += StartDeactivation;
    private void OnDisable() => GorillaBossAI.OnGorillaEnraged -= StartDeactivation;

    private void StartDeactivation() => StartCoroutine(DeactivateRoutine());

    IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        foreach (GameObject door in doors)
        {
            if (door != null)
            {
                door.SetActive(false);
                // Buraya kapı kırılma sesi veya partikül ekleyebilirsin
                yield return new WaitForSeconds(stepInterval);
            }
        }
    }
}