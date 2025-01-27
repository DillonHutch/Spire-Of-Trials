using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : EnemyParent
{
    protected override void Die()
    {
        base.Die();
        Debug.Log("Zombie defeated!");
        // Add custom logic for Zombie death, like spawning an additional zombie
    }
}
