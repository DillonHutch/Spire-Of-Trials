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
    [SerializeField] protected float attackIntervalMin = 0.5f;
    [SerializeField] protected float attackIntervalMax = 2f;
    [SerializeField] protected float windUpTime = 1f;
    protected bool isAttacking = false;
    protected int enemyAttackPosition;
    protected List<string> attackSequence = new List<string>();
    protected int currentSequenceIndex = 0;
    protected Coroutine attackCoroutine;
    protected int phaseSize = 4; // Default phase size, can be overridden by subclasses


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

    [SerializeField] private float flashTime;




    // Particle Effects
    [SerializeField] protected GameObject damageParticlePrefab; // Assign the prefab in the Inspector
    [SerializeField] protected GameObject partOrgin;

    // Animation
    protected Animator animator;

    // Position & Movement
    protected Vector3 originalPosition;
    protected SpriteRenderer spriteRenderer;



    // enemies that move support
    protected Transform leftSpawn;   // Left-side spawn position
    protected Transform centerSpawn; // Center spawn position
    protected Transform rightSpawn;  // Right-side spawn position
    protected Transform currentParent; // Stores the current parent transform




    // **Color Management**
    private Color originalColor;       // Stores the original color of the enemy
    private SpriteRenderer iconRenderer; // Reference to the icon sprite renderer
    private Color iconOriginalColor;    // Stores the original color of the icon

    // **References**
    protected Transform player;        // Reference to the player transform


    protected SheildsScript shieldManager;


    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is first initialized. 
    /// Sets up references, starts coroutines, and initializes health values.
    /// </summary>
    /// <summary>
    /// Called when the script instance is first initialized. 
    /// Sets up references, starts coroutines, and initializes health values.
    /// </summary>
    protected virtual void Start()
    {
        StartCoroutine(MonitorColorReset());

        // Cache commonly used components
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Store original values
        originalPosition = transform.position;
        originalColor = spriteRenderer.color;

        // Assign attack position
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        // Assign UI elements
        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider")?.GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();

        // Assign attack indicator renderer (with error checking)
        if (attackIndicator != null)
        {
            attackIndicatorRenderer = attackIndicator.GetComponent<SpriteRenderer>()
                ?? throw new MissingComponentException($"SpriteRenderer missing on {attackIndicator.name}.");
        }
        else
        {
            Debug.LogError($"attackIndicator is not assigned for {gameObject.name}. Assign it in the Inspector.");
        }

        // Assign icon renderer and store its original color
        iconRenderer = transform.childCount > 0 ? transform.GetChild(0).GetComponent<SpriteRenderer>() : null;
        iconOriginalColor = iconRenderer ? iconRenderer.color : Color.white;

        // Initialize attack sequence and update visuals
        DefineAttackSequence();
        UpdateColor();

        // Setup health bar values
        healthBar.maxValue = attackSequence.Count;
        healthBar.value = attackSequence.Count;

        // Handle attack coroutine
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoop());

        // Find the player in the scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        //find spawn
        leftSpawn = GameObject.FindGameObjectWithTag("LeftSpawn").GetComponent<Transform>();
        rightSpawn = GameObject.FindGameObjectWithTag("RightSpawn").GetComponent<Transform>();
        centerSpawn = GameObject.FindGameObjectWithTag("MiddleSpawn").GetComponent<Transform>();


        shieldManager = FindObjectOfType<SheildsScript>();

    }


    /// <summary>
    /// Called once per frame. Updates the health bar and color based on the current sequence index.
    /// </summary>
    protected virtual void Update()
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


    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {

            EventManager.Instance.StartListening<(
            SpriteRenderer left,
            SpriteRenderer center,
            SpriteRenderer right,
            Transform leftShield,
            Transform centerShield,
            Transform rightShield
        )>("InitializeAttackSprites", data =>
            InitializeAttackSprites(data.left, data.center, data.right, data.leftShield, data.centerShield, data.rightShield));





        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    /// <summary>
    /// Called when the GameObject is disabled. 
    /// Stops coroutines and resets relevant UI and visual elements.
    /// </summary>
    protected void OnDisable()
    {
        if (EventManager.Instance != null)
        {

            EventManager.Instance.StopListening<(
            SpriteRenderer left,
            SpriteRenderer center,
            SpriteRenderer right,
            Transform leftShield,
            Transform centerShield,
            Transform rightShield
        )>("InitializeAttackSprites", data =>
            InitializeAttackSprites(data.left, data.center, data.right, data.leftShield, data.centerShield, data.rightShield));



        }
        else
        {
            //Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }



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
    /// Initializes attack sprites and shields by assigning references.
    /// </summary>
    /// <param name="left">Left attack sprite.</param>
    /// <param name="center">Center attack sprite.</param>
    /// <param name="right">Right attack sprite.</param>
    /// <param name="lShield">Left shield transform.</param>
    /// <param name="cShield">Center shield transform.</param>
    /// <param name="rShield">Right shield transform.</param>
    void InitializeAttackSprites(SpriteRenderer left, SpriteRenderer center, SpriteRenderer right, Transform lShield, Transform cShield, Transform rShield)
    {
        leftAttackSprite = left;
        centerAttackSprite = center;
        rightAttackSprite = right;

        shieldManager?.InitializeShields(lShield, cShield, rShield);
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
    protected abstract int GetAttackPosition();


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
           // Debug.Log($"Next attack in {waitTime} seconds");

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
    protected virtual IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Select the attack sprite based on position
        int attackPosition = GetAttackPosition();
        SpriteRenderer attackSprite = GetAttackSprite(attackPosition);

        // Flip sprite if enemy is a goblin
        if (gameObject.CompareTag("Goblin"))
            spriteRenderer.flipX = attackPosition == 2;


        // Show attack sprite indicator
        if (attackSprite != null)
            StartCoroutine(ShowAttackIndicator(attackSprite));
        else
            Debug.LogError("Attack Sprite is NULL!");

        // Play wind-up animation and sound
        SetAnimationState("WindUp");
        WindUpSound();

        yield return new WaitForSeconds(windUpTime);



        // Check if the player successfully blocked the attack
        ResolveAttack(attackPosition);


        // Clear attack visuals
        yield return new WaitForSeconds(0.2f);
        CleanupAttack(attackSprite, attackPosition);

        // Notify attack queue
        EnemyAttackQueue.AttackFinished(this);
    }

    /// <summary>
    /// Gets the appropriate attack sprite based on position.
    /// </summary>
    protected SpriteRenderer GetAttackSprite(int attackPosition)
    {
        return attackPosition switch
        {
            0 => leftAttackSprite,
            1 => centerAttackSprite,
            2 => rightAttackSprite,
            _ => null
        };
    }

    /// <summary>
    /// Handles flashing the attack indicator.
    /// </summary>
    protected IEnumerator ShowAttackIndicator(SpriteRenderer attackSprite)
    {
        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;
        flashCoroutine = StartCoroutine(shieldManager.FlashAttackIndicator(attackSprite, this));

        yield return null;
    }

    /// <summary>
    /// Sets the animation states for wind-up and attack.
    /// </summary>
    private void SetAnimationState(string animationSequence)
    {
        animator.SetTrigger(animationSequence);
    }

    /// <summary>
    /// Determines whether the player dodged successfully and applies the appropriate effects.
    /// </summary>
    protected void ResolveAttack(int attackPosition)
    {
        int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

        AttackSound();

        if (playerDodgePosition == attackPosition)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
            //if (activeRecoilCoroutine != null) StopCoroutine(activeRecoilCoroutine);
            shieldManager?.TriggerShieldRecoil(attackPosition, this);

        }
        else
        {
            //Debug.Log("Player failed to block! Taking damage.");
            EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHit, transform.position);
        }

        //if(gameObject.tag == "Frog" && attackPosition == 0)
        //{
        //    spriteRenderer.flipX = false;
        //}
        //else if (gameObject.tag == "Frog" && attackPosition == 2)
        //{
        //    spriteRenderer.flipX = true;
        //}

        // Play attack animation and sound
        if (gameObject.tag == "Frog" && attackPosition == 0)
        {
            animator.SetTrigger("AttackLeft");

        }
        else if (gameObject.tag == "Frog" && attackPosition == 1)
        {
            animator.SetTrigger("AttackMiddle");
        }
        else if (gameObject.tag == "Frog" && attackPosition == 2)
        {
            animator.SetTrigger("AttackRight");
        }
        else
        {
            SetAnimationState("Attack");
        }



    }

    /// <summary>
    /// Cleans up the attack sequence, removing visuals and resetting state.
    /// </summary>
    protected void CleanupAttack(SpriteRenderer attackSprite, int attackPosition)
    {
        isAttacking = false;
        SetAnimationState("ReturnToIdle");
        dodgeBarHighlighter?.ClearHighlight(attackPosition);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (iconRenderer != null) iconRenderer.color = iconOriginalColor;

        shieldManager?.ResetShieldPositions();


        if (attackSprite != null)
        {
            attackSprite.enabled = false;
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
        }
    }


    /// <summary>
    /// Plays the wind-up sound effect based on the enemy type.
    /// </summary>
    protected void WindUpSound()
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
        else if (this.gameObject.tag == "Wendingo")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.wenWU, transform.position);
        }
        else if (this.gameObject.tag == "VineSerpant")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.serWU, transform.position);
        }
        else if (this.gameObject.tag == "Thornbrute")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.thornWU, transform.position);
        }
        else if (this.gameObject.tag == "Knight")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightWU, transform.position);
        }
        else if (this.gameObject.tag == "Frog")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.frogWU, transform.position);
        }
    }

    /// <summary>
    /// Plays the attack sound effect based on the enemy type.
    /// </summary>
    protected void AttackSound()
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
        else if (this.gameObject.tag == "Wendingo")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.wenAtk, transform.position);
        }
        else if (this.gameObject.tag == "VineSerpant")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.serAtk, transform.position);
        }
        else if (this.gameObject.tag == "Thornbrute")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.thornAtk, transform.position);
        }
        else if (this.gameObject.tag == "Knight")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightAttack, transform.position);
        }
        else if (this.gameObject.tag == "Frog")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.frogAttack, transform.position);
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
    protected virtual void UpdateColor()
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
    /// Handles the MiniBoss taking damage from the player.
    /// Uses a phase-based attack sequence where the MiniBoss must be attacked in a specific order.
    /// </summary>
    /// <param name="attackType">The type of attack the player used.</param>
    public virtual void TakeDamage(string attackType)
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
            //Debug.Log($"MiniBoss hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            // Play damage sound
            AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDamage, this.transform.position);

            // Flash red effect on hit
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRed());


            // Spawn damage particles
            if (damageParticlePrefab != null)
            {
                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f); // Cleanup after 0.5 sec
            }

            // If phase is completed, move to the next phase
            if (currentSequenceIndex >= phaseEndIndex)
            {
                //Debug.Log($"Phase {currentPhase + 1} completed!");
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


            if (player != null)
            {
                EventManager.Instance.TriggerEvent("UpdateCombo", true);
            }
        }
        else
        {
            Debug.Log("MiniBoss hit incorrectly! Resetting current phase.");

            // Reset only the current phase, not the entire sequence
            currentSequenceIndex = phaseStartIndex;
            UpdateColor();

            // Notify the player of a failed hit
            if (player != null)
            {
                EventManager.Instance.TriggerEvent("UpdateCombo", false);
            }
        }

        // Update the health bar based on attack sequence progress
        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex;
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
            Color storedMainColor = normalColor;
            Color storedIconColor = iconOriginalColor;

            spriteRenderer.color = damageColor;
            if (iconRenderer != null)
                iconRenderer.color = damageColor;

            yield return new WaitForSeconds(flashDuration);

            // Forcefully restore colors even if attacking
            spriteRenderer.color = storedMainColor;
            if (iconRenderer != null)
                iconRenderer.color = storedIconColor;
        }
    }


    /// <summary>
    /// Handles enemy death, including disabling UI elements, stopping effects, and destroying the object.
    /// </summary>
    protected virtual void Die()
    {
        //Debug.Log($"{gameObject.name} died!");

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

    #region MovementBetweenLocationMethods

    /// <summary>
    /// Sets the MiniBoss's new parent transform and moves it to the new position.
    /// Ensures the MiniBoss is correctly positioned within the spawn hierarchy.
    /// </summary>
    /// <param name="newParent">The new Transform parent.</param>
    protected void SetNewParent(Transform newParent)
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
    protected Transform GetRandomSpawn(params Transform[] positions)
    {
        return positions.Length > 0 ? positions[Random.Range(0, positions.Length)] : null;
    }

    #endregion

}

