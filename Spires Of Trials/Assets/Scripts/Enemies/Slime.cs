using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// is slime enemy 
/// </summary>
public class Slime : EnemyParent
{

    #region OverrideMethods

    /// <summary>
    /// Determines the attack position for the MiniBoss.
    /// If the enemy is at position 0 (left), it attacks position 2 (right).
    /// If the enemy is at position 2 (right), it attacks position 0 (left).
    /// The Slime does not attack when in position 1 (center).
    /// </summary>
    /// <returns>The attack position (0 or 2), or -1 if the attack is invalid.</returns>
    protected override int GetAttackPosition()
    {
        if (enemyAttackPosition == 0) return 2; // Attacks the right position if on the left
        if (enemyAttackPosition == 2) return 0; // Attacks the left position if on the right
        return -1; // No attack if in the center (position 1)
    }

    /// <summary>
    /// Defines the attack sequence for the Slime.
    /// Each attack follows a fixed pattern of "heavy", "magic", "melee", and "range".
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> { "heavy", "magic", "melee", "range" };
    }

    protected override void DefineEnemyAttackPattern()
    {
        // e.g. two parry attacks, then a dodge, then repeat
        enemyAttackPattern = new List<EnemyAttackType>
        {
            EnemyAttackType.Parry,
        };
    }

    #endregion
}
