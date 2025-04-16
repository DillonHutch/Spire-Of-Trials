using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleric : EnemyParent
{
    protected override int GetAttackPosition()
    {
        int randomAttack = Random.Range(0, 3);

        return randomAttack;

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
