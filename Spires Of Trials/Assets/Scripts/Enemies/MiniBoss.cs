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
    // **Serialized Fields** - Set in the Unity Inspector
    [SerializeField] private Transform leftSpawn;   // Left-side spawn position
    [SerializeField] private Transform centerSpawn; // Center spawn position
    [SerializeField] private Transform rightSpawn;  // Right-side spawn position

    // **References**
    private Transform player;        // Reference to the player transform
    private Transform currentParent; // Stores the current parent transform

    // **Color Management**
    private Color originalColor;       // Stores the original color of the enemy
    private SpriteRenderer iconRenderer; // Reference to the icon sprite renderer
    private Color iconOriginalColor;    // Stores the original color of the icon

    // **Coroutines**
    private Coroutine knightAttackCoroutine; // Coroutine reference for knight attack sequence

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes references before the game starts.
    /// </summary>
    private void Awake()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called before the first frame update.
    /// Initializes variables, assigns references, and sets up the enemy's spawn location.
    /// </summary>
    protected override void Start()
    {
        // Call base Start() to ensure parent class logic runs first
        base.Start();

        // Get the original color of the icon (if available) or default to white
        iconOriginalColor = iconRenderer != null ? iconRenderer.color : Color.white;

        // Store the original sprite color
        originalColor = spriteRenderer.color;

        // Assign iconRenderer by assuming the first child is the icon
        iconRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        // Set attack behavior properties
        attackIntervalMin = 1f; // Minimum time between attacks
        attackIntervalMax = 1f; // Maximum time between attacks
        windUpTime = 1f;        // Wind-up time before an attack executes

        // Find the player in the scene by tag
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Find spawn positions in the scene using tags
        leftSpawn = GameObject.FindWithTag("LeftSpawn")?.transform;
        centerSpawn = GameObject.FindWithTag("MiddleSpawn")?.transform;
        rightSpawn = GameObject.FindWithTag("RightSpawn")?.transform;

        // Error handling: Ensure all spawn points are assigned correctly
        if (leftSpawn == null || centerSpawn == null || rightSpawn == null)
        {
            Debug.LogError("MiniBoss spawn positions are not properly set! Check your tags.");
            return;
        }

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
        return enemyAttackPosition;
    }

    /// <summary>
    /// Overrides the standard attack loop to implement a burst attack pattern.
    /// The MiniBoss attacks rapidly for a short period before resting.
    /// </summary>
    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            int attackBurstCount = Random.Range(5, 8); // Number of rapid attacks before resting

            for (int i = 0; i < attackBurstCount; i++)
            {
                int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
                float attackDelay = Random.Range(0.3f, 0.5f); // Faster attack intervals

                yield return new WaitForSeconds(attackDelay); // Short delay between rapid attacks

                isAttacking = true;

                // Move MiniBoss to a random spawn point before attacking
                Transform randomSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
                SetNewParent(randomSpawn);

                // Start wind-up animation
                animator.SetTrigger("WindUp");

                // Choose a random attack position
                int miniBossTargetPos = Random.Range(0, 3);
                SpriteRenderer attackSprite = null;

                if (miniBossTargetPos == 0) attackSprite = leftAttackSprite;
                else if (miniBossTargetPos == 1) attackSprite = centerAttackSprite;
                else if (miniBossTargetPos == 2) attackSprite = rightAttackSprite;

                // Flash attack indicator before the attack
                if (attackSprite != null)
                {
                    attackSprite.enabled = true;
                    StartCoroutine(FlashAttackIndicator(attackSprite));
                }

                // Play wind-up sound
                AudioManager.instance.PlayOneShot(FMODEvents.instance.knightWU, transform.position);
                yield return new WaitForSeconds(0.5f); // Short wind-up time

                // Execute attack
                AudioManager.instance.PlayOneShot(FMODEvents.instance.knightAttack, transform.position);

                // Check if the player dodged correctly
                int updatedPlayerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
                if (updatedPlayerDodgePosition == miniBossTargetPos) // Player blocks correctly
                {
                    Debug.Log("Player successfully blocked the attack!");

                    // Stop any ongoing shield recoil coroutine
                    if (activeRecoilCoroutine != null) StopCoroutine(activeRecoilCoroutine);

                    // Trigger shield recoil effect
                    TriggerShieldRecoil(miniBossTargetPos);
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
                }
                else
                {
                    Debug.Log("Player failed to block! Taking damage from MiniBoss.");
                    EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.playerMetal, this.transform.position);
                }

                // Clear attack indicator highlight
                if (dodgeBarHighlighter != null)
                {
                    dodgeBarHighlighter.ClearHighlight(miniBossTargetPos);
                }

                // Trigger attack animation
                animator.SetTrigger("Attack");

                isAttacking = false;
                UpdateColor();

                // Fade out attack sprite after attack
                if (attackSprite != null)
                {
                    attackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);
                }

                yield return new WaitForSeconds(0.1f); // Small delay before resetting

                // Reset to idle before next attack
                animator.SetTrigger("ReturnToIdle");
                yield return new WaitForSeconds(0.1f); // Brief pause for animation reset
            }

            // **Rest Phase** - After the burst of attacks, the MiniBoss pauses
            Debug.Log("MiniBoss is resting...");
            animator.SetTrigger("ReturnToIdle"); // Reset to idle before resting
            yield return new WaitForSeconds(3f); // Rest period for punishment window
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

    /// <summary>
    /// Coroutine to make the attack indicator sprite flash three times.
    /// Used to alert the player before an incoming attack.
    /// </summary>
    protected override IEnumerator FlashRoutine(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;

        Color originalColor = attackSprite.color;

        // Ensure sprite is enabled before flashing
        attackSprite.enabled = true;

        for (int i = 0; i < 3; i++) // Flash 3 times
        {
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, warningOpacity); // Slightly transparent
            yield return new WaitForSeconds(0.1f);
            attackSprite.color = originalColor; // Reset
            yield return new WaitForSeconds(0.1f);
        }

        // Keep enabled for the next attack
        attackSprite.enabled = true;

        flashCoroutine = null;
    }

    /// <summary>
    /// Handles the MiniBoss taking damage from the player.
    /// Uses a phase-based attack sequence where the MiniBoss must be attacked in a specific order.
    /// </summary>
    /// <param name="attackType">The type of attack the player used.</param>
    new public void TakeDamage(string attackType)
    {
        PlayerAttackingScript player = FindObjectOfType<PlayerAttackingScript>(); // Find the player script

        int phaseSize = 4; // Each phase consists of 4 attacks
        int totalPhases = attackSequence.Count / phaseSize;
        int currentPhase = currentSequenceIndex / phaseSize; // Determine which phase the player is in
        int phaseStartIndex = currentPhase * phaseSize; // Start of the current phase
        int phaseEndIndex = phaseStartIndex + phaseSize; // End of the current phase

        // Check if the attack matches the expected sequence
        if (currentSequenceIndex < attackSequence.Count && attackType == attackSequence[currentSequenceIndex])
        {
            currentSequenceIndex++;
            Debug.Log($"MiniBoss hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            // Play damage sound
            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDamage, this.transform.position);

            // Flash red effect on hit
            StartCoroutine(FlashRed());

            // Spawn damage particles
            if (damageParticlePrefab != null)
            {
                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f); // Cleanup after 0.5 sec
            }

            // If phase is completed, move to the next phase
            if (currentSequenceIndex >= phaseEndIndex)
            {
                Debug.Log($"Phase {currentPhase + 1} completed!");
            }

            // If all phases are completed, the MiniBoss dies
            if (currentSequenceIndex >= attackSequence.Count)
            {
                Die();
            }
            else
            {
                UpdateColor();
            }

            // Notify the player of a successful combo hit
            player?.UpdateCombo(true);
        }
        else
        {
            Debug.Log("MiniBoss hit incorrectly! Resetting current phase.");

            // Reset only the current phase, not the entire sequence
            currentSequenceIndex = phaseStartIndex;
            UpdateColor();

            // Notify the player of a failed hit
            player?.UpdateCombo(false);
        }

        // Update the health bar based on attack sequence progress
        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex;
        }
    }

    #endregion

    #region KnightAttackSequence

    /// <summary>
    /// Updates the attack indicator sprite and animation parameters based on the next attack in the sequence.
    /// Ensures the correct attack type is displayed and properly animated.
    /// </summary>
    private void UpdateColor()
    {
        // If all attacks have been completed, return early
        if (currentSequenceIndex >= attackSequence.Count) return;

        // Ensure the attack indicator renderer exists before modifying it
        if (attackIndicatorRenderer == null)
        {
            // Debug.LogError($"{gameObject.name}: attackIndicatorRenderer is NULL. Ensure attackIndicator has a SpriteRenderer.");
            return;
        }

        // Get the next attack type in the sequence
        string nextAttack = attackSequence[currentSequenceIndex];

        // Assign the correct sprite and update the animator's "AttackType" float for animations
        switch (nextAttack)
        {
            case "melee":
                attackIndicatorRenderer.sprite = meleeSprite;
                animator.SetFloat("AttackType", 0f);
                break;
            case "magic":
                attackIndicatorRenderer.sprite = magicSprite;
                animator.SetFloat("AttackType", 0.33f);
                break;
            case "range":
                attackIndicatorRenderer.sprite = rangeSprite;
                animator.SetFloat("AttackType", 0.66f);
                break;
            case "heavy":
                attackIndicatorRenderer.sprite = heavySprite;
                animator.SetFloat("AttackType", 1f);
                break;
        }
    }

    /// <summary>
    /// Retrieves the corresponding spawn transform based on a given index.
    /// Ensures valid values by defaulting to the center spawn if an invalid index is provided.
    /// </summary>
    /// <param name="index">The spawn position index (0 = left, 1 = center, 2 = right).</param>
    /// <returns>The corresponding Transform spawn point.</returns>
    private Transform GetSpawnFromIndex(int index)
    {
        switch (index)
        {
            case 0: return leftSpawn;
            case 1: return centerSpawn;
            case 2: return rightSpawn;
            default: return centerSpawn; // Default to center if an invalid index is given
        }
    }

    /// <summary>
    /// Sets the MiniBoss's new parent transform and moves it to the new position.
    /// Ensures the MiniBoss is correctly positioned within the spawn hierarchy.
    /// </summary>
    /// <param name="newParent">The new Transform parent.</param>
    private void SetNewParent(Transform newParent)
    {
        if (newParent != null)
        {
            currentParent = newParent;
            transform.position = currentParent.position; // Move to new position
            transform.SetParent(currentParent); // Set hierarchy parent
        }
    }

    /// <summary>
    /// Selects a random spawn point from the given list of spawn positions.
    /// Used for randomizing the MiniBoss's movement between attacks.
    /// </summary>
    /// <param name="positions">An array of possible spawn positions.</param>
    /// <returns>A randomly selected Transform spawn position.</returns>
    private Transform GetRandomSpawn(params Transform[] positions)
    {
        return positions.Length > 0 ? positions[Random.Range(0, positions.Length)] : null;
    }

    /// <summary>
    /// Determines the closest spawn position relative to the player's current position.
    /// Used for dynamically positioning the MiniBoss based on the player's movement.
    /// </summary>
    /// <returns>The Transform of the closest spawn position.</returns>
    private Transform GetPlayerPosition()
    {
        return player.position.x < centerSpawn.position.x
            ? leftSpawn
            : (player.position.x > centerSpawn.position.x ? rightSpawn : centerSpawn);
    }

    /// <summary>
    /// Converts a spawn Transform into an integer index.
    /// Helps in mapping Transform positions to numerical values for logic handling.
    /// </summary>
    /// <param name="position">The Transform position to convert.</param>
    /// <returns>The corresponding index (0 = left, 1 = center, 2 = right).</returns>
    private int GetPositionIndex(Transform position)
    {
        if (position == leftSpawn) return 0;
        if (position == centerSpawn) return 1;
        if (position == rightSpawn) return 2;
        return 1; // Default to center if the position is unrecognized
    }

    #endregion

}

