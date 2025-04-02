using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : EnemyParent
{
    #region Override Methods

    /// <summary>
    /// Determines the attack position for the Skeleton.
    /// </summary>
    /// <returns>The enemy's current attack position.</returns>
    protected override int GetAttackPosition()
    {
        int randomAttack = Random.Range(0, 3);

        return randomAttack; // Attack the position in front
    }

    /// <summary>
    /// Defines the attack sequence for the Skeleton.
    /// The sequence follows a structured pattern of "melee", "heavy", "range", and "magic" attacks.
    /// This ensures a variety of attack types in a predictable order.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string> { "melee", "heavy", "range", "magic", };
    }

    private int GetAttackBurstCount() => Random.Range(5, 8); // MiniBoss attacks in bursts

    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            int attackBurstCount = GetAttackBurstCount();

            for (int i = 0; i < attackBurstCount; i++)
            {
                // Determine a random attack interval within the min/max range, rounded to one decimal place
                float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
                yield return new WaitForSeconds(waitTime);              
                yield return PerformAttack(); // Reuse parent attack logic with minor tweaks

            }

            // **Rest Phase** - MiniBoss pauses after its attack burst
            Debug.Log("MiniBoss is resting...");

            animator.SetTrigger("ReturnToIdle");
           
            
            yield return new WaitForSeconds(3f); // Punishment window
        }
    }


 


    #endregion
}
