using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Devil : EnemyParent
{
    protected override int GetAttackPosition()
    {
        int attackPos;

        if (enemyAttackPosition == 0)
        {
            attackPos = Random.Range(1, 3);
            spriteRenderer.flipX = true;
            return attackPos;
        }


        if (enemyAttackPosition == 1)
        {
            attackPos = Random.Range(0, 2) == 0 ? 0 : 2;
            if (attackPos == 0)
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
            attackPos = Random.Range(0, 2);
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
                "melee", "magic", "range", "heavy",   // Combo 1
                "range", "melee", "heavy", "magic"    // Combo 2  
                };
    }
}
