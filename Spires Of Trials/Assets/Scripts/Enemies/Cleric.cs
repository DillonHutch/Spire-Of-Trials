using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleric : EnemyParent
{
    protected override int GetAttackPosition()
    {
        int randomAttack = Random.Range(0, 3);

        return randomAttack;

    }


    /// <summary>
    /// Defines the attack sequence for the Goblin.
    /// The sequence follows a rotating pattern of "magic", "range", "heavy", and "melee" attacks.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> {
                "range", "heavy", "melee", "magic",   // Combo 1  
                "melee", "magic", "range", "heavy",   // Combo 2  
                "magic", "range", "heavy", "melee"    // Combo 3  

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
