using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thornbrute : EnemyParent
{
    protected override int GetAttackPosition()
    {
        int attackPos;

        if (enemyAttackPosition == 0)
        {
            attackPos = Random.Range(0, 2);
            spriteRenderer.flipX = true;
            return attackPos;
        } 
           
        
        if (enemyAttackPosition == 1)
        {
            attackPos = Random.Range(0, 3);
            if(attackPos == 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
            return attackPos;
        }

        if (enemyAttackPosition == 2)
        {
            attackPos = Random.Range(1, 3);
            return attackPos;
        }

        return 1;
  
    }

    /// <summary>
    /// Defines the attack sequence for the Goblin.
    /// The sequence follows a rotating pattern of "magic", "range", "heavy", and "melee" attacks.
    /// </summary>
    protected override void DefineAttackSequence()
    {
                 attackSequence = new List<string> {
                "magic", "range", "melee", "heavy",   // Combo 1
                "melee", "magic", "range", "heavy"    // Combo 2
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
