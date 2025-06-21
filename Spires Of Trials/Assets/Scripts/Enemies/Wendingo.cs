using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wendingo : EnemyParent
{

    int attackInRow = 0;

    protected override int GetAttackPosition()
    {
        int attackAmount = 3; // Positions: 0, 1, 2

        int currentAttack = attackInRow;
        attackInRow = (attackInRow + 1) % attackAmount;

        return currentAttack;

    }


    /// <summary>
    /// Defines the attack sequence for the Goblin.
    /// The sequence follows a rotating pattern of "magic", "range", "heavy", and "melee" attacks.
    /// </summary>
    protected override void DefineAttackSequence()
    {
                attackSequence = new List<string> {
                "melee", "magic", "range", "heavy",   // Combo 1
                "range", "melee", "heavy", "magic"    // Combo 2  
                };
    }


    protected override void DefineEnemyAttackPattern()
    {
        // e.g. two parry attacks, then a dodge, then repeat
        enemyAttackPattern = new List<EnemyAttackType>
        {
            EnemyAttackType.Parry,
        };
    }
}
