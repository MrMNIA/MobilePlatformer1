using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private LayerMask damageLayer; // buraya hem Player, hem Enemy eklenebilir.
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if ((damageLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.GetComponent<Health>()?.SuddenDeath();
        }
    }
}