using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : EnemyParent
{

    private List<int> weakSpotsPerHit = new List<int>();
    private List<string> weakSpotColorsPerHit = new List<string>();


    private int currentWeakSpotPosition = 1; // Default to center

    public enum WeakSpotPosition
    {
        Left = 0,
        Right = 1,
        Top = 2
    }



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
        attackSequence = new List<string> {
        "melee", "range", "magic", "heavy",
        "melee", "magic", "range", "heavy",
        "melee", "range", "heavy", "magic",
    };

        weakSpotsPerHit = new List<int>();
        weakSpotColorsPerHit = new List<string>();

        for (int i = 0; i < attackSequence.Count; i++)
        {
            weakSpotsPerHit.Add(Random.Range(0, 3)); // Random weak spot (0 = left, 1 = top, 2 = right)

            string atkType = attackSequence[i];
            string color = atkType switch
            {
                "melee" => "red",
                "magic" => "blue",
                "range" => "green",
                "heavy" => "yellow",
                _ => "red"
            };
            weakSpotColorsPerHit.Add(color);

            //Debug.LogError(atkType);
           // Debug.LogError(color);
        }


    }





    private float GetBlendValue(int position, string color)
    {
        Dictionary<string, float[]> colorMap = new Dictionary<string, float[]>
    {
        { "blue", new float[] { 0.00f, 0.37f, 0.73f } },    // magic
        { "green", new float[] { 0.10f, 0.46f, 0.82f } },   // range
        { "red", new float[] { 0.20f, 0.55f, 0.91f } },     // melee
        { "yellow", new float[] { 0.30f, 0.64f, 1.00f } },  // heavy
    };

        return colorMap[color][position];
    }



    private int GetAttackBurstCount() => Random.Range(5, 8); // MiniBoss attacks in bursts

    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            int attackBurstCount = GetAttackBurstCount();

            for (int i = 0; i < attackBurstCount; i++)
            {
                if (currentSequenceIndex < weakSpotsPerHit.Count)
                {
                    int fakePos = weakSpotsPerHit[currentSequenceIndex];
                    SetFakePosition(fakePos);
                    UpdateColor();
                }

                float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
                yield return new WaitForSeconds(waitTime);

                yield return PerformAttack();
            }

            // 💤 Rest Phase
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle");
            yield return new WaitForSeconds(3f);

            // 🛠️ Prepare next weak spot visuals after rest
            if (currentSequenceIndex < weakSpotsPerHit.Count)
            {
                int fakePos = weakSpotsPerHit[currentSequenceIndex];
                SetFakePosition(fakePos);
                UpdateColor();
            }
        }

    }


    protected override void Start()
    {
        base.Start();

        if (weakSpotsPerHit.Count > 0)
        {
            SetFakePosition(weakSpotsPerHit[0]);
            UpdateColor();
        }

    }

    public override void TakeDamage(string attackType)
    {
        PlayerAttackingScript player = FindObjectOfType<PlayerAttackingScript>();

        int playerPosition = player != null ? player.GetCurrentAttackPosition() : -1;





        int expectedPosition = weakSpotsPerHit[currentSequenceIndex];
        SetFakePosition(expectedPosition);

        if (currentSequenceIndex < attackSequence.Count &&
    attackType == attackSequence[currentSequenceIndex] &&
    playerPosition == expectedPosition)
        {
            currentSequenceIndex++;
            //Debug.LogError($"Frog hit correctly at weak spot {playerPosition}! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDamage, transform.position);
            StartCoroutine(FlashRed());

            if (damageParticlePrefab != null)
            {
                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f);
            }

            // ✅ Immediately prep next weak spot here
            if (currentSequenceIndex < weakSpotsPerHit.Count)
            {
                SetFakePosition(weakSpotsPerHit[currentSequenceIndex]);
                UpdateColor();
            }

            if (currentSequenceIndex >= attackSequence.Count)
            {
                Die();
            }

            EventManager.Instance.TriggerEvent("UpdateCombo", true);
        }
        else
        {
            //Debug.LogError("Incorrect attack or wrong position! Resetting hit.");
            UpdateColor();
            EventManager.Instance.TriggerEvent("UpdateCombo", false);
        }



        // Grab the correct weak spot for THIS attack


        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex;
        }
    }


    /// <summary>
    /// Updates the attack indicator sprite and animation parameters based on the next attack in the sequence.
    /// Ensures the correct attack type is displayed and properly animated.
    /// </summary>
    protected override void UpdateColor()
    {
        base.UpdateColor();

        if (currentSequenceIndex < weakSpotColorsPerHit.Count)
        {
            int pos = GetCurrentFakePosition(); // ✅ use actual fake position
            string color = weakSpotColorsPerHit[currentSequenceIndex];
            float blendVal = GetBlendValue(pos, color);
            animator.SetFloat("WeaknessBlend", blendVal);
        }
    }

    private void SetFakePosition(int pos)
    {
        Transform targetParent = pos switch
        {
            0 => leftSpawn,
            1 => centerSpawn,
            2 => rightSpawn,
            _ => centerSpawn,
        };

        if (targetParent != null)
        {
            transform.SetParent(targetParent);
        }
    }

    private int GetCurrentFakePosition()
    {
        if (transform.parent == leftSpawn) return 0;
        if (transform.parent == centerSpawn) return 1;
        if (transform.parent == rightSpawn) return 2;

        return 1; // Default to center if unknown
    }




    #endregion



}
