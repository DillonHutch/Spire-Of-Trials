using System.Collections.Generic;
using UnityEngine;

public class Goblin : EnemyParent
{
    protected override int GetAttackPosition()
    {
        if (enemyAttackPosition == 0) return 1;
        if (enemyAttackPosition == 2) return 1;
        return Random.Range(0, 2) == 0 ? 0 : 2; // 50/50 chance to attack 0 or 2

        //return enemyAttackPosition;
    }


    protected override void DefineAttackSequence()
    {

        attackSequence = new List<string> { "magic", "range", "heavy", "melee" };
              
    }

}
