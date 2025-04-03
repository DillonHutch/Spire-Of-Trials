using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineSerpent : EnemyParent
{
    /// <summary>
    /// Determines the attack position for the Goblin.
    /// - If the enemy is at position 0 (left), it attacks position 1 (center).
    /// - If the enemy is at position 2 (right), it attacks position 1 (center).
    /// - If the enemy is at position 1 (center), it randomly attacks position 0 (left) or position 2 (right) with a 50/50 chance.
    /// </summary>
    /// <returns>The attack position (0, 1, or 2), ensuring logical attack behavior.</returns>
    protected override int GetAttackPosition()
    {
        if (enemyAttackPosition == 0) return 1; // Attacks center if on the left
        if (enemyAttackPosition == 2) return 1; // Attacks center if on the right

        // If in the center (position 1), randomly attack left (0) or right (2)
        return 1;
    }

    /// <summary>
    /// Defines the attack sequence for the Goblin.
    /// The sequence follows a rotating pattern of "magic", "range", "heavy", and "melee" attacks.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> { "heavy", "range", "heavy", "melee", "magic", "melee", "heavy", "melee" };
    }
}
