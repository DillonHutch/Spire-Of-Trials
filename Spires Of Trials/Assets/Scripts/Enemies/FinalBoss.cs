using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : EnemyParent
{
    #region Fields

     private List<Transform> spawnPoints;
    [SerializeField] private GameObject[] spawnableEnemies;
    [SerializeField] private float spawnEnemyChance = 0.25f;
    [SerializeField] private float despawnDelay = 10f;

    private List<GameObject> summonedEnemies = new List<GameObject>();

    private EnemySpawner enemySpawner;



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

        enemySpawner = FindObjectOfType<EnemySpawner>();


        spawnPoints = new List<Transform> { leftSpawn, rightSpawn, centerSpawn };

        Debug.Log("Spawn points initialized:");
        foreach (Transform point in spawnPoints)
            Debug.Log(point.name);

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
    private int GetAttackBurstCount() => Random.Range(1,1); // MiniBoss attacks in bursts

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
                if (randomSpawn == centerSpawn)
                {
                    transform.localScale = new Vector3(.8f, .8f, .8f);
                    transform.localPosition = new Vector3(0, -.9f, .0f);
                }
                else
                {
                    transform.localScale = new Vector3(.9f, .9f, .9f);
                    transform.localPosition = new Vector3(-.6f, 0f, .0f);
                }
                SetNewParent(randomSpawn);
                yield return PerformAttack(); // Reuse parent attack logic with minor tweaks

            }

            
            
            

            // **Rest Phase** - MiniBoss pauses after its attack burst
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle");

            //enemySpawner.ForceSpawnEnemy();


            yield return new WaitForSeconds(3f); // Punishment window
        }
    }


    private void TrySpawnEnemyInOtherLane()
    {

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != this.transform.parent) // Not the boss's current lane
            {
                if (spawnPoints[i].childCount == 1) // Space is empty
                {
                    enemySpawner.ForceSpawnEnemy();
                }
            }
        }
    }




    private IEnumerator DespawnAfterDelay(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemy != null)
        {
            Destroy(enemy);
            summonedEnemies.Remove(enemy);
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




    #endregion
}
