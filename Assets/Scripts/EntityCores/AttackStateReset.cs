using UnityEngine;

public class AttackStateReset : StateMachineBehaviour
{
    // OnStateExit yerine OnStateEnter (yeni bir duruma girince) 
    // veya OnStateExit'i daha hızlı tetiklemek için:
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 'animator.GetComponent<EnemyAI>()' hem EnemyAI hem de GorillaBossAI'yı bulur.
        // Çünkü GorillaBossAI bir EnemyAI'dır.
        var enemy = animator.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.isAttacking = false;
            // Goril'e özel bir sıfırlama varsa (mesela safety timer) onu da temizle
            animator.SendMessage("Event_FinishAttack", SendMessageOptions.DontRequireReceiver);
            Debug.Log("<color=white>SMB:</color> Saldırı durumu başarıyla sıfırlandı.");
        }
    }
}