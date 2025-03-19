using System.Collections.Generic;
using UnityEngine;

public class Slime : EnemyParent
{
    protected override int GetAttackPosition()
    {
        if (enemyAttackPosition == 0) return 2;
        if (enemyAttackPosition == 2) return 0;
        return -1; // Invalid attack position (Monster does not attack if at position 1)
        //return enemyAttackPosition;
    }

    protected override void DefineAttackSequence()
    {
      
       attackSequence = new List<string> { "heavy", "magic", "melee", "range" };
               
    }


}
