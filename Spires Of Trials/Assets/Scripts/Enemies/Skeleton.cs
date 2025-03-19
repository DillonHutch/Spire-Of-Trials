using System.Collections.Generic;
using UnityEngine;

public class Skeleton : EnemyParent
{
    protected override int GetAttackPosition()
    {
        return enemyAttackPosition; // Attack the position in front
    }

    protected override void DefineAttackSequence()
    {

        attackSequence = new List<string> { "melee", "heavy", "range", "magic" };
          
    }

}


