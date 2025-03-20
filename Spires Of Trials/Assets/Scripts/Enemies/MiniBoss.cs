using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// For the Knight MiniBoss
/// </summary>
public class MiniBoss : EnemyParent
{

    #region Fields


    #endregion

    #region UnityMethods

    /// <summary>
    /// Called before the first frame update.
    /// Initializes variables, assigns references, and sets up the enemy's spawn location.
    /// </summary>
    protected override void Start()
    {
        // Call base Start() to ensure parent class logic runs first
        base.Start(); 

        // Set a random spawn position as the new parent
        SetNewParent(GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn));

    }

    #endregion

    #region OverrideRegions

    /// <summary>
    /// returns position of miniboss
    /// </summary>
    /// <returns></returns>
    protected override int GetAttackPosition()
    {
        return Random.Range(0, 3);
    }

    /// <summary>
    /// Amount of burst attacks miniboss will throw
    /// </summary>
    /// <returns></returns>
    private int GetAttackBurstCount() => Random.Range(5, 8); // MiniBoss attacks in bursts

    /// <summary>
    /// Continuously loops and waits for a random interval before requesting an attack.
    /// Ensures each attack happens at a randomized interval within the given range.
    /// </summary>
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
                Transform randomSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
                SetNewParent(randomSpawn);
                yield return PerformAttack(); // Reuse parent attack logic with minor tweaks
                
            }

            // **Rest Phase** - MiniBoss pauses after its attack burst
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle");
            yield return new WaitForSeconds(3f); // Punishment window
        }
    }


    /// <summary>
    /// Defines the MiniBoss's attack sequence.
    /// Consists of repeated attack types in a structured order.
    /// </summary>
    protected override void DefineAttackSequence()
    {
        attackSequence = new List<string>
    {
        "melee", "magic", "range", "heavy",
        "magic", "melee", "range", "heavy",
        "melee", "magic", "range", "heavy",
        "melee", "range", "magic", "heavy",
        "melee", "magic", "range", "heavy",
        "magic", "melee", "range", "heavy",
        "melee", "magic", "range", "heavy",
        "magic", "melee", "range", "heavy"
    };
    }

   

    #endregion

    #region KnightAttackSequence

    /// <summary>
    /// Updates the attack indicator sprite and animation parameters based on the next attack in the sequence.
    /// Ensures the correct attack type is displayed and properly animated.
    /// </summary>
    protected override void UpdateColor()
    {
        base.UpdateColor();

        string changeColor = attackSequence[currentSequenceIndex];

        switch (changeColor)
        {
            case "melee":              
                animator.SetFloat("AttackType", 0f);
                break;
            case "magic":
                animator.SetFloat("AttackType", 0.33f);
                break;
            case "range":
                animator.SetFloat("AttackType", 0.66f);
                break;
            case "heavy":
                animator.SetFloat("AttackType", 1f);
                break;
        }
    }


   

    #endregion

}

