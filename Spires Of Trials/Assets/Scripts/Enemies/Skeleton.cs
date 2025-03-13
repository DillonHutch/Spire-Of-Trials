using UnityEngine;

public class Skeleton : EnemyParent
{
    protected override int GetAttackPosition()
    {
        return enemyAttackPosition; // Attack the position in front
    }

   
}
