using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wendingo : EnemyParent
{

    int attackInRow = 0;

    protected override int GetAttackPosition()
    {
        return attackInRow;
    }


    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            int attackAmount = 3;

            for (int i = 0; i < attackAmount; i++)
            {
                // Determine a random attack interval within the min/max range, rounded to one decimal place
                float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
                yield return new WaitForSeconds(waitTime);

                yield return PerformAttack(); // Reuse parent attack logic with minor tweaks

                attackInRow++;  

            }

            // **Rest Phase** - MiniBoss pauses after its attack burst
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle");
            attackInRow = 0;
            yield return new WaitForSeconds(3f); // Punishment window
        }
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
