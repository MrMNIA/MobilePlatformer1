using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;
    public Health playerHealth;
    public PlayerEnergy playerEnergy;
    private bool isRespawned = false;
    public void GetLastValidPosition(Vector3 position)
    {
        respawnPoint = position;
    }

    public bool IsRespawned()
    {
        return isRespawned;
    }
    public void Respawn()
    {
        if (!isRespawned)
        {
            transform.position = respawnPoint; // Karakteri son geçerli pozisyona taşı
            playerHealth.StartRespawn(); // Sağlığı sıfırla
            playerEnergy.FillEnergy(playerEnergy.maxEnergy); // Enerjiyi tam doldur
            isRespawned = true; // Respawn işlemi gerçekleşti
        }
    }
}
