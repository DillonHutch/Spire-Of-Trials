using UnityEngine;

public class Monster : EnemyParent
{
    protected override int GetAttackPosition()
    {
        if (enemyAttackPosition == 0) return 2;
        if (enemyAttackPosition == 2) return 0;
        return -1; // Invalid attack position (Monster does not attack if at position 1)
        //return enemyAttackPosition;
    }

  
}
