using UnityEngine;

public class EnrageTeleportTrigger : MonoBehaviour
{
    public Transform playerTransform;

    private void OnEnable() => GorillaBossAI.OnGorillaEnraged += TeleportToPlayer;
    private void OnDisable() => GorillaBossAI.OnGorillaEnraged -= TeleportToPlayer;

    private void TeleportToPlayer()
    {
        if (playerTransform != null)
        {
            // Trigger'ı oyuncunun tam konumuna ışınla
            transform.position = playerTransform.position;
            Debug.Log("<color=cyan>TRIGGER:</color> Player konumuna ışınlandım!");
        }
    }
}