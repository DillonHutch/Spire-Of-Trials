using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : EnemyParent
{
    protected override void Die()
    {
        base.Die();
        Debug.Log("Skeleton defeated!");
        // Add custom logic for Skeleton death, like dropping loot
    }
}
