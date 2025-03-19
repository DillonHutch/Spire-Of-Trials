using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the queue system for enemy attacks.
/// Ensures that only one enemy attacks at a time, while others wait their turn.
/// </summary>
public class EnemyAttackQueue : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Queue of enemies waiting to attack.
    /// </summary>
    private static Queue<EnemyParent> attackQueue = new Queue<EnemyParent>();

    /// <summary>
    /// The enemy that is currently attacking.
    /// </summary>
    private static EnemyParent currentAttackingEnemy;

    #endregion

    #region Queue Management

    /// <summary>
    /// Requests an enemy to be added to the attack queue.
    /// If the enemy is not already in the queue, it gets enqueued and the attack cycle starts if possible.
    /// </summary>
    /// <param name="enemy">The enemy requesting to attack.</param>
    public static void RequestAttack(EnemyParent enemy)
    {
        // Ensure the enemy is valid and active in the scene before adding to queue
        if (enemy == null || !enemy.gameObject.activeInHierarchy) return;

        // Only enqueue the enemy if it is not already in the queue
        if (!attackQueue.Contains(enemy))
        {
            attackQueue.Enqueue(enemy);
            TryStartNextAttack(); // Attempt to start the next attack if no attack is ongoing
        }
    }

    /// <summary>
    /// Attempts to start the next enemy attack from the queue.
    /// Ensures only one enemy attacks at a time.
    /// </summary>
    private static void TryStartNextAttack()
    {
        // If an enemy is already attacking, do not start a new attack
        if (currentAttackingEnemy != null) return;

        // Process the attack queue
        while (attackQueue.Count > 0)
        {
            EnemyParent nextEnemy = attackQueue.Dequeue(); // Get the next enemy in queue

            // Ensure the enemy is still valid and active before starting its attack
            if (nextEnemy != null && nextEnemy.gameObject.activeInHierarchy)
            {
                currentAttackingEnemy = nextEnemy;
                currentAttackingEnemy.StartAttack(); // Initiate attack
                return;
            }
        }

        // If no valid enemies remain, clear the queue to prevent stale references
        attackQueue.Clear();
    }

    #endregion

    #region Attack Completion

    /// <summary>
    /// Marks an enemy's attack as finished and starts the next attack if available.
    /// </summary>
    /// <param name="enemy">The enemy that has finished its attack.</param>
    public static void AttackFinished(EnemyParent enemy)
    {
        // Ensure that the current attacking enemy is the one finishing the attack
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null; // Reset the current attacker
            TryStartNextAttack(); // Start the next attack if possible
        }
    }

    #endregion

    #region Enemy Removal

    /// <summary>
    /// Removes an enemy from the attack queue if it is destroyed or no longer valid.
    /// Ensures no stale references remain in the queue.
    /// </summary>
    /// <param name="enemy">The enemy to remove from the queue.</param>
    public static void RemoveEnemy(EnemyParent enemy)
    {
        // Remove the enemy from the queue if it exists
        if (attackQueue.Contains(enemy))
        {
            List<EnemyParent> updatedQueue = new List<EnemyParent>(attackQueue);
            updatedQueue.Remove(enemy);
            attackQueue = new Queue<EnemyParent>(updatedQueue); // Rebuild the queue without the removed enemy
        }

        // If the removed enemy was the current attacker, reset and continue
        if (currentAttackingEnemy == enemy)
        {
            currentAttackingEnemy = null;
            TryStartNextAttack();
        }
    }

    #endregion
}
