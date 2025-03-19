using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemy Parent Class
/// </summary>
public abstract class EnemyParent : MonoBehaviour
{
    #region Fields
    // Health and UI Elements
    [SerializeField] protected Slider healthBar;
    [SerializeField] protected Image healthBarFill; // Assign the Fill Image in Inspector
    [SerializeField] protected Gradient healthGradient; // Create a Gradient in Inspector

    // Dodge Mechanic
    protected Slider dodgeSlider;
    protected DodgeBarHighlighter dodgeBarHighlighter;

    // Colors
    protected Color normalColor = Color.white;
    protected Color damageColor = Color.red;
    protected float warningOpacity = 0.50f;

    // Flash Effect
    protected Coroutine flashCoroutine;
    protected float flashDuration = 0.2f;

    // Attack System
    protected float attackIntervalMin = 0.5f;
    protected float attackIntervalMax = 2f;
    protected float windUpTime = 1f;
    protected bool isAttacking = false;
    protected int enemyAttackPosition;
    protected List<string> attackSequence = new List<string>();
    protected int currentSequenceIndex = 0;
    protected Coroutine attackCoroutine;

    // Attack Indicators
    [SerializeField] protected GameObject attackIndicator; // Assign in Inspector (e.g., an empty GameObject with a SpriteRenderer)
    [SerializeField] protected Sprite meleeSprite;
    [SerializeField] protected Sprite magicSprite;
    [SerializeField] protected Sprite rangeSprite;
    [SerializeField] protected Sprite heavySprite;
    protected SpriteRenderer attackIndicatorRenderer;

    // Attack Sprites
    protected SpriteRenderer leftAttackSprite;
    protected SpriteRenderer centerAttackSprite;
    protected SpriteRenderer rightAttackSprite;

    // Shields
    protected Transform leftShield;
    protected Transform centerShield;
    protected Transform rightShield;

    // Particle Effects
    [SerializeField] protected GameObject damageParticlePrefab; // Assign the prefab in the Inspector
    [SerializeField] protected GameObject partOrgin;

    // Animation
    protected Animator animator;

    // Position & Movement
    protected Vector3 originalPosition;
    protected SpriteRenderer spriteRenderer;

    // Recoil Mechanic
    protected Coroutine activeRecoilCoroutine;
    private bool isRecoiling = false;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is first initialized. 
    /// Sets up references, starts coroutines, and initializes health values.
    /// </summary>
    protected virtual void Start()
    {
        // Start monitoring color resets
        StartCoroutine(MonitorColorReset());

        // Get the attack position from the parent spawn point
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        // Find and assign the dodge slider and highlighter
        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();

        // Cache the sprite renderer and original position
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;

        // Cache the animator component
        animator = GetComponent<Animator>();

        // Ensure attackIndicator is properly set up
        if (attackIndicator != null)
        {
            attackIndicatorRenderer = attackIndicator.GetComponent<SpriteRenderer>();

            // Log an error if the SpriteRenderer is missing
            if (attackIndicatorRenderer == null)
            {
                Debug.LogError($"SpriteRenderer missing on {attackIndicator.name}. Please add one.");
            }
        }
        else
        {
            // Log an error if attackIndicator is not assigned
            Debug.LogError($"attackIndicator is not assigned for {gameObject.name}. Assign it in the Inspector.");
        }

        // Define the attack sequence for the enemy
        DefineAttackSequence();

        // Update the enemy's color based on its state
        UpdateColor();

        // Stop any existing attack coroutine and start a new attack loop
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoop());

        // Set up health bar values
        healthBar.maxValue = attackSequence.Count;
        healthBar.value = attackSequence.Count; // Start full
    }

    /// <summary>
    /// Called once per frame. Updates the health bar and color based on the current sequence index.
    /// </summary>
    protected void Update()
    {
        if (healthBar != null)
        {
            // Update health bar value based on remaining attack sequence
            healthBar.value = attackSequence.Count - currentSequenceIndex;
            float healthPercentage = healthBar.value / healthBar.maxValue;

            // Update the health bar fill color based on remaining health
            healthBarFill.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    /// <summary>
    /// Called when the GameObject is disabled. 
    /// Stops coroutines and resets relevant UI and visual elements.
    /// </summary>
    protected void OnDisable()
    {
        // Stop the attack coroutine if it is running
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        // Ensure the dodge bar highlight is cleared when the enemy is disabled
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

        // Ensure attack indicator flash coroutine is stopped
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Ensure attack indicator visuals are hidden when the enemy is disabled
        if (leftAttackSprite != null)
            leftAttackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);

        if (centerAttackSprite != null)
            centerAttackSprite.color = new Color(centerAttackSprite.color.r, centerAttackSprite.color.g, centerAttackSprite.color.b, 0f);

        if (rightAttackSprite != null)
            rightAttackSprite.color = new Color(rightAttackSprite.color.r, rightAttackSprite.color.g, rightAttackSprite.color.b, 0f);
    }

    #endregion

    #region ShieldMethods

    /// <summary>
    /// Triggers the shield recoil effect at a given position if the shield is active.
    /// Prevents multiple recoils from happening simultaneously.
    /// </summary>
    /// <param name="position">The position of the shield (0 = left, 1 = center, 2 = right).</param>
    protected void TriggerShieldRecoil(int position)
    {
        // Get the shield transform at the given position
        Transform shieldToRecoil = GetShieldByPosition(position);

        // Ensure the shield exists and is active before applying recoil
        if (shieldToRecoil != null && shieldToRecoil.gameObject.activeSelf)
        {
            Debug.Log($"Attempting to trigger shield recoil at position {position}");

            // Prevent multiple recoil effects from running at the same time
            if (!isRecoiling)
            {
                isRecoiling = true; // Lock recoil state
                activeRecoilCoroutine = StartCoroutine(ShieldRecoil(shieldToRecoil));
            }
        }
    }

    /// <summary>
    /// Coroutine that temporarily moves the shield downward and then resets its position.
    /// </summary>
    /// <param name="shield">The shield transform to apply the recoil effect.</param>
    protected IEnumerator ShieldRecoil(Transform shield)
    {
        // Store the shield's original position
        Vector3 originalPosition = shield.position;

        // Calculate the recoil position (slightly lower)
        Vector3 recoilPosition = originalPosition + new Vector3(0, -0.2f, 0);

        Debug.Log($"Recoil Start for {shield.name} at {shield.position}");

        // Move the shield down slightly to simulate impact
        shield.position = recoilPosition;
        yield return new WaitForSeconds(0.1f); // Short recoil delay

        // Reset shield to its original position
        shield.position = originalPosition;
        Debug.Log($"Recoil End for {shield.name}");

        // Unlock recoil state to allow future recoils
        isRecoiling = false;
        activeRecoilCoroutine = null; // Clear coroutine reference
    }

    /// <summary>
    /// Triggers a flashing effect on the attack indicator sprite.
    /// Ensures previous flash coroutines are stopped before starting a new one.
    /// </summary>
    /// <param name="attackSprite">The attack indicator sprite to flash.</param>
    protected IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite)
    {
        // Ensure the sprite exists before attempting to flash
        if (attackSprite == null) yield break;

        // Make sure the sprite is visible before flashing
        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;

        // Stop any existing flash coroutine to prevent overlapping effects
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Start the flashing effect
        flashCoroutine = StartCoroutine(FlashRoutine(attackSprite));
    }

    /// <summary>
    /// Coroutine that makes the attack indicator sprite flash three times.
    /// </summary>
    /// <param name="attackSprite">The attack indicator sprite to flash.</param>
    protected virtual IEnumerator FlashRoutine(SpriteRenderer attackSprite)
    {
        // Ensure the sprite exists before attempting to flash
        if (attackSprite == null) yield break;

        // Store the sprite's original color
        Color originalColor = attackSprite.color;

        // Ensure the sprite is enabled before starting the flash effect
        attackSprite.enabled = true;

        // Perform three flashes
        for (int i = 0; i < 3; i++)
        {
            // Change the sprite color to a semi-transparent state
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, warningOpacity);
            yield return new WaitForSeconds(0.15f);

            // Reset to original color
            attackSprite.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }

        // Keep the sprite enabled for the next attack indication
        attackSprite.enabled = true;

        // Clear coroutine reference when done
        flashCoroutine = null;
    }

    /// <summary>
    /// Retrieves the shield transform corresponding to a given position.
    /// </summary>
    /// <param name="position">The shield position (0 = left, 1 = center, 2 = right).</param>
    /// <returns>Transform of the corresponding shield or null if the position is invalid.</returns>
    protected Transform GetShieldByPosition(int position)
    {
        switch (position)
        {
            case 0: return leftShield;
            case 1: return centerShield;
            case 2: return rightShield;
            default: return null;
        }
    }

    /// <summary>
    /// Initializes attack sprites and shields by assigning references.
    /// </summary>
    /// <param name="left">Left attack sprite.</param>
    /// <param name="center">Center attack sprite.</param>
    /// <param name="right">Right attack sprite.</param>
    /// <param name="lShield">Left shield transform.</param>
    /// <param name="cShield">Center shield transform.</param>
    /// <param name="rShield">Right shield transform.</param>
    public void InitializeAttackSprites(SpriteRenderer left, SpriteRenderer center, SpriteRenderer right, Transform lShield, Transform cShield, Transform rShield)
    {
        leftAttackSprite = left;
        centerAttackSprite = center;
        rightAttackSprite = right;

        leftShield = lShield;
        centerShield = cShield;
        rightShield = rShield;
    }

    #endregion

    #region EnemyAttacking

    /// <summary>
    /// Defines the attack sequence for the enemy.
    /// This method should be implemented in child classes.
    /// </summary>
    protected abstract void DefineAttackSequence();

    /// <summary>
    /// Returns the enemy's attack position.
    /// Can be overridden by subclasses if needed.
    /// </summary>
    /// <returns>Integer representing the attack position.</returns>
    protected virtual int GetAttackPosition()
    {
        return enemyAttackPosition;
    }

    /// <summary>
    /// Continuously loops and waits for a random interval before requesting an attack.
    /// Ensures each attack happens at a randomized interval within the given range.
    /// </summary>
    protected virtual IEnumerator AttackLoop()
    {
        while (true)
        {
            // Determine a random attack interval within the min/max range, rounded to one decimal place
            float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
            Debug.Log($"Next attack in {waitTime} seconds");

            yield return new WaitForSeconds(waitTime);

            // Request to attack
            EnemyAttackQueue.RequestAttack(this);

            // Ensure waitTime applies before restarting the loop
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Starts the attack process by beginning the attack coroutine.
    /// </summary>
    public void StartAttack()
    {
        StartCoroutine(PerformAttack());
    }

    /// <summary>
    /// Handles the entire attack sequence, including wind-up, attack execution, and attack resolution.
    /// </summary>
    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Determine which attack sprite to use based on attack position
        int attackPosition = GetAttackPosition();
        SpriteRenderer attackSprite = null;

        if (attackPosition == 0) attackSprite = leftAttackSprite;
        else if (attackPosition == 1) attackSprite = centerAttackSprite;
        else if (attackPosition == 2) attackSprite = rightAttackSprite;

        // Flip the sprite direction if necessary (e.g., Goblin should face the correct way)
        if (gameObject.tag == "Goblin")
        {
            spriteRenderer.flipX = attackPosition == 2;
        }

        // Highlight the attack position on the dodge bar
        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.HighlightPosition(attackPosition);

        // Flash attack indicator if an attack sprite exists
        if (attackSprite != null)
        {
            Debug.Log($"Starting Flash for {attackSprite.gameObject.name}");

            // Ensure sprite is visible before flashing
            attackSprite.gameObject.SetActive(true);
            attackSprite.enabled = true;

            StartCoroutine(FlashAttackIndicator(attackSprite));
        }
        else
        {
            Debug.LogError("Attack Sprite is NULL!");
        }

        // Trigger wind-up animation
        animator.SetBool("IsWinding", true);
        animator.SetBool("IsAttacking", false); // Ensure it's false before the attack

        // Play wind-up sound effect based on enemy type
        WindUpSound();

        // Wait for wind-up duration
        yield return new WaitForSeconds(windUpTime);

        // Stop wind-up animation and trigger attack animation
        animator.SetBool("IsWinding", false);
        animator.SetBool("IsAttacking", true);

        // Play attack sound effect based on enemy type
        AttackSound();

        // Determine the player's dodge position
        int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

        // Check if the player successfully blocked the attack
        if (playerDodgePosition == attackPosition)
        {
            // Play shield block sound
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);

            // Trigger shield recoil effect
            if (activeRecoilCoroutine != null) StopCoroutine(activeRecoilCoroutine);
            TriggerShieldRecoil(attackPosition);
        }
        else
        {
            // Player failed to block - take damage
            Debug.Log("Player failed to block! Taking damage.");
            EventManager.Instance.TriggerEvent("takeDamageEvent", 1);

            // Play damage sound effect
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerMetal, transform.position);
        }

        // Clear dodge bar highlight after attack
        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.ClearHighlight(attackPosition);

        // Allow a brief delay for the attack animation to play out
        yield return new WaitForSeconds(0.2f);

        // Reset attack state and animation
        isAttacking = false;
        animator.SetBool("IsAttacking", false);

        // Ensure the attack indicator is turned off after the attack ends
        if (attackSprite != null)
        {
            attackSprite.enabled = false; // Hide attack sprite

            // Stop flashing if the coroutine is still running
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
        }

        // Notify the attack queue that the attack is finished
        EnemyAttackQueue.AttackFinished(this);
    }

    /// <summary>
    /// Plays the wind-up sound effect based on the enemy type.
    /// </summary>
    void WindUpSound()
    {
        if (this.gameObject.tag == "Goblin")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.gobWU, transform.position);
        }
        else if (this.gameObject.tag == "Skeleton")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.skeWU, transform.position);
        }
        else if (this.gameObject.tag == "Slime")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.slimeWU, transform.position);
        }
    }

    /// <summary>
    /// Plays the attack sound effect based on the enemy type.
    /// </summary>
    void AttackSound()
    {
        if (this.gameObject.tag == "Goblin")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.gobAtk, transform.position);
        }
        else if (this.gameObject.tag == "Skeleton")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.skeAtk, transform.position);
        }
        else if (this.gameObject.tag == "Slime")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.slimeAtk, transform.position);
        }
    }

    #endregion

    #region EnemyTakingDamage

    /// <summary>
    /// Monitors and resets the enemy's color back to normal if it's not attacking.
    /// Ensures visual feedback (such as damage flashes) revert correctly over time.
    /// </summary>
    private IEnumerator MonitorColorReset()
    {
        // Get the icon renderer (assumes the first child is the icon)
        SpriteRenderer iconRenderer = transform.childCount > 0 ? transform.GetChild(0).GetComponent<SpriteRenderer>() : null;

        while (true)
        {
            yield return new WaitForSeconds(0.5f); // Check color reset every 0.5 seconds

            // Reset the main sprite color if it's different from normal and the enemy is not attacking
            if (spriteRenderer.color != normalColor && !isAttacking)
            {
                spriteRenderer.color = normalColor;
            }

            // Reset the icon's color if it exists and is different from normal
            if (iconRenderer != null && iconRenderer.color != normalColor && !isAttacking)
            {
                iconRenderer.color = normalColor;
            }
        }
    }

    /// <summary>
    /// Updates the attack indicator sprite based on the next attack in the sequence.
    /// Ensures the enemy displays the correct attack type.
    /// </summary>
    private void UpdateColor()
    {
        // If the enemy has completed all attacks, do nothing
        if (currentSequenceIndex >= attackSequence.Count) return;

        // Ensure the attack indicator renderer exists
        if (attackIndicatorRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: attackIndicatorRenderer is NULL. Ensure attackIndicator has a SpriteRenderer.");
            return;
        }

        // Get the next attack type in the sequence
        string nextAttack = attackSequence[currentSequenceIndex];

        // Assign the appropriate sprite based on the attack type
        switch (nextAttack)
        {
            case "melee":
                attackIndicatorRenderer.sprite = meleeSprite;
                break;
            case "magic":
                attackIndicatorRenderer.sprite = magicSprite;
                break;
            case "range":
                attackIndicatorRenderer.sprite = rangeSprite;
                break;
            case "heavy":
                attackIndicatorRenderer.sprite = heavySprite;
                break;
        }
    }

    /// <summary>
    /// Handles taking damage and verifying if the player's attack was correct.
    /// Updates attack sequence progress or resets if the attack was incorrect.
    /// </summary>
    /// <param name="attackType">The type of attack the player used.</param>
    public void TakeDamage(string attackType)
    {
        // Get a reference to the player's attack script
        PlayerAttackingScript player = FindObjectOfType<PlayerAttackingScript>();

        // Check if the attack matches the expected sequence
        if (currentSequenceIndex < attackSequence.Count && attackType == attackSequence[currentSequenceIndex])
        {
            currentSequenceIndex++; // Advance attack sequence
            Debug.Log($"{gameObject.name} hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            // Trigger red flash effect to indicate a successful hit
            StartCoroutine(FlashRed());

            // Spawn damage particles at the particle origin point
            if (damageParticlePrefab != null)
            {
                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f); // Auto-cleanup after 0.5 seconds
            }

            // Notify the player of a successful combo hit
            player?.UpdateCombo(true);

            // If the attack sequence is complete, the enemy dies
            if (currentSequenceIndex >= attackSequence.Count)
            {
                Die();
            }
            else
            {
                // Update attack indicator to show the next expected attack
                UpdateColor();
            }
        }
        else
        {
            // Incorrect attack resets the sequence
            Debug.Log($"{gameObject.name} hit incorrectly! Resetting sequence.");
            currentSequenceIndex = 0;

            // Reset health bar if it exists
            if (healthBar != null)
                healthBar.value = 0;

            // Notify the player of a failed hit (miss)
            player?.UpdateCombo(false);

            // Update attack indicator to restart the sequence
            UpdateColor();
        }
    }

    /// <summary>
    /// Coroutine to briefly flash the enemy red when hit.
    /// Provides visual feedback for taking damage.
    /// </summary>
    protected IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            // Store original colors
            Color originalColor = spriteRenderer.color;
            SpriteRenderer iconRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Assumes the first child is the icon
            Color iconOriginalColor = iconRenderer != null ? iconRenderer.color : Color.white;

            // Change to red to indicate damage
            spriteRenderer.color = Color.red;
            if (iconRenderer != null)
                iconRenderer.color = Color.red;

            yield return new WaitForSeconds(0.2f); // Damage flash duration

            // Restore original colors
            spriteRenderer.color = originalColor;
            if (iconRenderer != null)
                iconRenderer.color = iconOriginalColor;
        }
    }

    /// <summary>
    /// Handles enemy death, including disabling UI elements, stopping effects, and destroying the object.
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Disable attack indicator before enemy is destroyed
        if (attackIndicatorRenderer != null)
        {
            attackIndicatorRenderer.enabled = false;
        }

        // Hide attack sprites if they were in use
        if (leftAttackSprite != null)
            leftAttackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);

        if (centerAttackSprite != null)
            centerAttackSprite.color = new Color(centerAttackSprite.color.r, centerAttackSprite.color.g, centerAttackSprite.color.b, 0f);

        if (rightAttackSprite != null)
            rightAttackSprite.color = new Color(rightAttackSprite.color.r, rightAttackSprite.color.g, rightAttackSprite.color.b, 0f);

        // Destroy the health bar UI if it exists
        if (healthBar != null)
            Destroy(this.healthBar.gameObject);

        // Ensure dodge bar highlight is cleared before destruction
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

        // Stop any running flash effect coroutine
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Notify the attack manager that this enemy is no longer active
        EnemyAttackQueue.AttackFinished(this);

        // Destroy the enemy game object
        Destroy(gameObject);
    }

    #endregion

}
