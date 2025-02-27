using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackQueue : MonoBehaviour
{
    private static Queue<EnemyParent> attackQueue = new Queue<EnemyParent>();
    private static EnemyParent currentAttackingEnemy;

    public static void RequestAttack(EnemyParent enemy)
    {
        if (enemy == null || !enemy.gameObject.activeInHierarchy) return; // Ensure the enemy is valid

        if (!attackQueue.Contains(enemy))
        {
            attackQueue.Enqueue(enemy);
            TryStartNextAttack();
        }
    }

    private static void TryStartNextAttack()
    {
        if (currentAttackingEnemy != null) return; // Don't start a new attack if one is ongoing

        while (attackQueue.Count > 0)
        {
            EnemyParent nextEnemy = attackQueue.Dequeue();

            if (nextEnemy != null && nextEnemy.gameObject.activeInHierarchy) // Ensure enemy is still valid
            {
                currentAttackingEnemy = nextEnemy;
                currentAttackingEnemy.StartAttack();
                return;
            }
        }

        // If no valid enemies remain, clear the queue to prevent stale references
        attackQueue.Clear();
    }

    public static void AttackFinished(EnemyParent enemy)
    {
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
            TryStartNextAttack();
        }
    }

    public static void RemoveEnemy(EnemyParent enemy)
    {
        if (attackQueue.Contains(enemy))
        {
            List<EnemyParent> updatedQueue = new List<EnemyParent>(attackQueue);
            updatedQueue.Remove(enemy);
            attackQueue = new Queue<EnemyParent>(updatedQueue);
        }

        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
            TryStartNextAttack();
        }
    }
}
