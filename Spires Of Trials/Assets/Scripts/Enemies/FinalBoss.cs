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
    private Transform currentLane; // This is the actual spawn lane the boss is in


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


        currentLane = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);

        SetNewParent(currentLane);
        


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
    private int GetAttackBurstCount() => Random.Range(5,8); // MiniBoss attacks in bursts

    /// <summary>
    /// Continuously loops and waits for a random interval before requesting an attack.
    /// Ensures each attack happens at a randomized interval within the given range.
    /// </summary>
    protected override IEnumerator AttackLoop()
    {
        while (true)
        {

            // Determine a random attack interval within the min/max range, rounded to one decimal place
            float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
            // Debug.Log($"Next attack in {waitTime} seconds");

            yield return new WaitForSeconds(waitTime);

            int attackOrConjure = Random.Range(1, 101);


            // Request to attack
            if(attackOrConjure < 85)
            {
                Transform randomSpawn = null;
                foreach (Transform lane in spawnPoints)
                {
                    if (lane != currentLane && IsLaneEmpty(lane))
                    {
                        randomSpawn = lane;
                        break;
                    }
                }

                // If no other empty lane is found, stay in current lane
                if (randomSpawn == null)
                {
                    randomSpawn = currentLane;
                }

                SetNewParent(randomSpawn);
                currentLane = randomSpawn;
                if (randomSpawn == centerSpawn)
                {
                    transform.localScale = new Vector3(.8f, .8f, .8f);
                    transform.localPosition = new Vector3(0, -.9f, .0f);
                    
                }
                else if (randomSpawn == leftSpawn)
                {
                    transform.localScale = new Vector3(.9f, .9f, .9f);
                    transform.localPosition = new Vector3(0, 0f, .0f);
                   spriteRenderer.flipX = true;
                    foreach (Transform child in transform)
                    {
                        Vector3 localPos = child.localPosition;
                        localPos.x = -.9f; // Flip X position
                        child.localPosition = localPos;
                    }


                }
                else if (randomSpawn == rightSpawn)
                {

                    transform.localScale = new Vector3(.9f, .9f, .9f);
                    transform.localPosition = new Vector3(-.6f, 0f, .0f);
                    spriteRenderer.flipX = false;



                }

             
                   
                EnemyAttackQueue.RequestAttack(this);
            }
            else
            {
                animator.SetTrigger("Conjure");
                TrySpawnEnemyInOtherLane();
                yield return new WaitForSeconds(1f);

                animator.SetTrigger("ReturnToIdle");
                yield return new WaitForSeconds(.5f);

                
            }
            

            // Ensure waitTime applies before restarting the loop
            yield return new WaitForSeconds(waitTime);

           


        }
    }


    private void TrySpawnEnemyInOtherLane()
    {
        KillAllOtherEnemies();
        enemySpawner.ForceSpawnEnemy();
    }

    private void KillAllOtherEnemies()
    {
        EnemyParent[] allEnemies = FindObjectsOfType<EnemyParent>();

        foreach (EnemyParent enemy in allEnemies)
        {
            if (enemy != this)
            {
                Destroy(enemy.gameObject);
            }
        }
    }



    private bool IsLaneEmpty(Transform lane)
    {
        foreach (Transform child in lane)
        {
            if (child.GetComponent<EnemyParent>() != null && child.gameObject != this.gameObject)
            {
                return false;
            }
        }
        return true;
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

            

// Set 1 – Alphabetical order permutations
"heavy", "magic", "melee", "range",
"heavy", "melee", "magic", "range",
"magic", "heavy", "range", "melee",
"melee", "range", "magic", "heavy",

// Set 2 – Reverse and rotate
"magic", "range", "melee", "heavy",
"range", "melee", "heavy", "magic",
"melee", "heavy", "magic", "range",
"heavy", "magic", "range", "melee",

// Set 3 – No repeats across positions (e.g., each slot gets a different lead once)
"melee", "range", "magic", "heavy",
"range", "magic", "heavy", "melee",
"magic", "heavy", "melee", "range",
"heavy", "melee", "range", "magic",

// Set 4 – Paired swapping (swap pairs)
"melee", "magic", "range", "heavy",
"magic", "melee", "heavy", "range",
"range", "heavy", "melee", "magic",
"heavy", "range", "magic", "melee",

// Set 5 – Alternating magic/melee front
"magic", "range", "melee", "heavy",
"melee", "magic", "range", "heavy",
"magic", "heavy", "range", "melee",
"melee", "range", "heavy", "magic",

// Set 6 – Center-focus (always put magic in 2nd)
"melee", "magic", "range", "heavy",
"range", "magic", "heavy", "melee",
"heavy", "magic", "melee", "range",
"magic", "magic", "melee", "range",  // intentional double for pattern break

// Set 7 – Cascading (each moves one step right each combo)
"melee", "range", "magic", "heavy",
"heavy", "melee", "range", "magic",
"magic", "heavy", "melee", "range",
"range", "magic", "heavy", "melee",

// Set 8 – Magic always last
"melee", "heavy", "range", "magic",
"range", "melee", "heavy", "magic",
"heavy", "range", "melee", "magic",
"magic", "melee", "range", "magic",  // magic start + end for twist

// Set 9 – Palindrome pattern-ish (first = last, 2nd = 3rd)
"melee", "range", "range", "melee",
"heavy", "magic", "magic", "heavy",
"range", "melee", "melee", "range",
"magic", "heavy", "heavy", "magic",

// Set 10 – Criss-cross groups (every second slot rotates in opposite order)
"melee", "magic", "range", "heavy",
"range", "heavy", "melee", "magic",
"heavy", "range", "magic", "melee",
"magic", "melee", "heavy", "range",



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
