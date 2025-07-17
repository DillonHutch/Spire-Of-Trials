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

    BattleSceneController battleController;


    // track whether we’re on left (0), center (1) or right (2)
    private int currentSpawnIndex;

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
        Transform initialSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
        SetNewParent(initialSpawn);

        // scale down if it’s the center spawn, otherwise reset to full size
        transform.localScale = (initialSpawn == centerSpawn)
            ? Vector3.one * 0.8f
            : Vector3.one * 0.9f;



        // Faster attack settings
        attackIntervalMin = 0.4f;
        attackIntervalMax = 0.6f;
        windUpTime = 0.5f;

        battleController = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleSceneController>();

        AudioManager.instance.SetMusic(MusicEnum.RuinsBoss);


    }

    #endregion

    #region OverrideRegions


    protected override void SetNewParent(Transform newParent)
    {
        base.SetNewParent(newParent);

        if (newParent == leftSpawn) currentSpawnIndex = 0;
        else if (newParent == centerSpawn) currentSpawnIndex = 1;
        else if (newParent == rightSpawn) currentSpawnIndex = 2;
    }


    /// <summary>
    /// returns position of miniboss
    /// </summary>
    /// <returns></returns>
    protected override int GetAttackPosition()
    {
        return currentSpawnIndex;
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
    // in AttackLoop()
    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            int attackBurstCount = GetAttackBurstCount();

            for (int i = 0; i < attackBurstCount; i++)
            {
                float waitTime = Mathf.Round(Random.Range(
                    attackIntervalMin, attackIntervalMax
                ) * 10f) / 10f;
                yield return new WaitForSeconds(waitTime);

                Transform spawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
                SetNewParent(spawn);

                // if it’s center (position 1) scale to 0.8, else back to 1
                transform.localScale = (spawn == centerSpawn)
                    ? Vector3.one * 0.8f
                    : Vector3.one * 0.9f;

                yield return PerformAttack();
            }

            // rest phase
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle");
            
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

            

         // Original 6
    "melee", "magic", "range", "heavy",
    "magic", "melee", "range", "heavy",
    "range", "magic", "melee", "heavy",
    "magic", "range", "melee", "heavy",
    "range", "melee", "magic", "heavy",
    "melee", "range", "magic", "heavy",


        // Variants with intention (e.g., alternating starts)
    "melee", "magic", "range", "heavy",
    "range", "melee", "magic", "heavy",
    "magic", "range", "melee", "heavy",
    "melee", "range", "magic", "heavy",
    "range", "magic", "melee", "heavy",
    "magic", "melee", "range", "heavy",

    // New unique-feel combos (repeat permutations, vary rhythm/order)
    "melee", "magic", "range", "heavy",   // repeat of 1
    "magic", "melee", "range", "heavy",   // repeat of 2
    "melee", "range", "magic", "heavy",   // repeat of 6
    "range", "magic", "melee", "heavy",   // repeat of 3
    "magic", "range", "melee", "heavy",   // repeat of 4
    "range", "melee", "magic", "heavy",   // repeat of 5


    };
    }


    protected override void Die()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDeath, transform.position);
        battleController.EndBattle();
        base.Die();

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

    protected override void DefineEnemyAttackPattern()
    {
        // e.g. two parry attacks, then a dodge, then repeat
        enemyAttackPattern = new List<EnemyAttackType>
        {
            EnemyAttackType.Dodge,
            EnemyAttackType.Dodge,
            EnemyAttackType.Dodge,
            EnemyAttackType.Parry,
            EnemyAttackType.Crouch,
            EnemyAttackType.Crouch,
            EnemyAttackType.Parry,
        };
    }


    #endregion

}

