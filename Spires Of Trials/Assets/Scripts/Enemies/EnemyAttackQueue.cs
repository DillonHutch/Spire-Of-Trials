using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackQueue : MonoBehaviour
{
    private static Queue<EnemyParent> attackQueue = new Queue<EnemyParent>();
    private static EnemyParent currentAttackingEnemy;

    public static void RequestAttack(EnemyParent enemy)
    {
        if (!attackQueue.Contains(enemy))
        {
            attackQueue.Enqueue(enemy);
            TryStartNextAttack();
        }
    }

    private static void TryStartNextAttack()
    {
        if (currentAttackingEnemy == null && attackQueue.Count > 0)
        {
            currentAttackingEnemy = attackQueue.Dequeue();
            currentAttackingEnemy.StartAttack();
        }
    }

    public static void AttackFinished(EnemyParent enemy)
    {
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
            TryStartNextAttack();
        }
    }
}
