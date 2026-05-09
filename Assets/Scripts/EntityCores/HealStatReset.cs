using UnityEngine;

public class HealStateReset : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator'ın olduğu objedeki EnemyAI scriptini bul ve değişkeni sıfırla
        var enemy = animator.GetComponent<ShamanEnemy>();
        if (enemy != null)
        {
            enemy.isHealing = false;
        }
    }
}