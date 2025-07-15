using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// is skeleton
/// </summary>
public class Skeleton : EnemyParent
{
    #region Override Methods

    /// <summary>
    /// Determines the attack position for the Skeleton.
    /// </summary>
    /// <returns>The enemy's current attack position.</returns>
    protected override int GetAttackPosition()
    {
        return enemyAttackPosition; // Attack the position in front
    }

    /// <summary>
    /// Defines the attack sequence for the Skeleton.
    /// The sequence follows a structured pattern of "melee", "heavy", "range", and "magic" attacks.
    /// This ensures a variety of attack types in a predictable order.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> { 
            //"melee", "heavy", "range", "magic",
            //"melee", "range", "heavy", "magic",
            //"melee", "range", "melee", "range",
            //"magic", "range", "heavy", "melee",

            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",
            "melee", "range", "melee", "range",

        };
    }

    protected override void DefineEnemyAttackPattern()
    {
        // e.g. two parry attacks, then a dodge, then repeat
        enemyAttackPattern = new List<EnemyAttackType>
        {
            EnemyAttackType.Dodge,
            EnemyAttackType.Parry,
            EnemyAttackType.Crouch
        };
    }


    #endregion

}


