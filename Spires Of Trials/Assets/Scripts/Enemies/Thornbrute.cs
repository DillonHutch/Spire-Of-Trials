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
            return attackPos;
        } 
           
        
        if (enemyAttackPosition == 1)
        {
            attackPos = Random.Range(0, 3);
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
        attackSequence = new List<string> { "heavy", "range", "heavy", "melee", "magic", "melee", "heavy", "melee" };
    }
}
