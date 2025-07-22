using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public enum EnemyAttackType
{
    Shield,   // player must block with shield/parry
    Dodge,   // player must move the dodge slider away
    Parry   // hold button down when enemy does a sweeping attack to the player 

}

/// <summary>
/// Enemy Parent Class
/// </summary>
public abstract class EnemyParent : MonoBehaviour
{
    #region Fields

    [Header("Parry Settings")]
     protected float parryWindow = 0.5f;  // length of the input window in seconds
    [SerializeField] protected float parryBonusTime = 5f;    // seconds to add to your timer



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
     protected float attackIntervalMin = 1f;
     protected float attackIntervalMax = 1f;
     protected float windUpTime = 1f;
    public bool isAttacking = false;
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

    private bool fightStarted;


    protected List<EnemyAttackType> enemyAttackPattern = new List<EnemyAttackType>();
    protected int enemyPatternIndex = 0;

    private float normalIntervalMin;
    private float normalIntervalMax;
    private float normalWindUpTime;
    private bool isSlowed = false;


    public bool IsAttacking
    {
        get { return isAttacking; }

    }

    private bool parryWindowActive = false;


    [Header("Next‐Hit Reveal UI")]
    [SerializeField] private SpriteRenderer nextHitIndicator;
    private bool revealNextActive = false;

    private int revealOffset = 1;


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


        normalIntervalMin = attackIntervalMin;
        normalIntervalMax = attackIntervalMax;
        normalWindUpTime = windUpTime;

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
        //attackCoroutine = StartCoroutine(AttackLoop());

        // Find the player in the scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        //find spawn
        leftSpawn = GameObject.FindGameObjectWithTag("LeftSpawn").GetComponent<Transform>();
        rightSpawn = GameObject.FindGameObjectWithTag("RightSpawn").GetComponent<Transform>();
        centerSpawn = GameObject.FindGameObjectWithTag("MiddleSpawn").GetComponent<Transform>();


        shieldManager = FindObjectOfType<SheildsScript>();

        if (TimingController.Instance.FightActive)
            StartFight();

        DefineEnemyAttackPattern();

    }

    // cache your sprites in one place
    private Sprite GetSpriteFor(string atk)
    {
        switch (atk)
        {
            case "melee": return meleeSprite;
            case "magic": return magicSprite;
            case "range": return rangeSprite;
            case "heavy": return heavySprite;
            default: return null;
        }
    }


    /// <summary>
    /// Turn the small “next hit” icon on/off, and immediately update its sprite.
    /// </summary>
    public void ShowNextHitIndicator(bool show, int offset = 1)
    {
        revealOffset = offset;
        revealNextActive = show;
        UpdateNextHitIcon();
    }

    /// <summary>
    /// Recompute which attack is coming up next,
    /// and set the little icon’s sprite+enabled state.
    /// </summary>
    // Change this signature from private → public
    public void UpdateNextHitIcon()
    {
        // if we’re not in “reveal” mode, hide the icon
        if (!revealNextActive)
        {
            nextHitIndicator.enabled = false;
            return;
        }

        // grab the current skipNextHit value
        bool skip = FindObjectOfType<FightController>().skipNextHit;
        int offset = skip ? 2 : 1;

        // if we’d go past the end, hide
        if (currentSequenceIndex + offset >= attackSequence.Count)
        {
            nextHitIndicator.enabled = false;
            return;
        }

        // otherwise pick the sprite that many steps ahead
        string next = attackSequence[currentSequenceIndex + offset];
        nextHitIndicator.sprite = GetSpriteFor(next);
        nextHitIndicator.enabled = (nextHitIndicator.sprite != null);
    }



    public void StartFight()
    {
        if (!fightStarted)
        {
            fightStarted = true;
            attackCoroutine = StartCoroutine(AttackLoop());
        }
    }


    public void StopFight()
    {
        if(fightStarted)
        {
            fightStarted = false;
            animator.SetTrigger("ReturnToIdle");
            StopCoroutine(attackCoroutine);
        }
    }



    /// <summary>
    /// Called once per frame. Updates the health bar and color based on the current sequence index.
    /// </summary>
    protected virtual void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space)
                && !parryWindowActive
                && !shieldManager.ParryInProgress
                && !TimingController.Instance.FightPanelUp)
        {
            shieldManager.TriggerGlobalParry();
        }


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


            EventManager.Instance.StartListening("OnStartFight", StartFight);
            EventManager.Instance.StartListening("OnStopFight", StopFight);

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

            EventManager.Instance.StopListening("OnStartFight", StartFight);
            EventManager.Instance.StopListening("OnStopFight", StopFight);

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

    //protected virtual void OnDestroy()
    //{
    //    // if this enemy was ever in the queue, make sure it's un‑queued
    //    EnemyAttackQueue.AttackFinished(this);
    //}


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
    /// Child classes implement this to say when they want parry vs dodge attacks.
    /// </summary>
    protected abstract void DefineEnemyAttackPattern();


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
        // only run while the fight is actually active
        while (TimingController.Instance.FightActive)
        {
            // pick a random interval
            float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
            yield return new WaitForSeconds(waitTime);

            // if we’ve been told the fight’s over, stop here
            if (!TimingController.Instance.FightActive)
                yield break;

            EnemyAttackQueue.RequestAttack(this);
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

        // pick this turn’s attack type
        EnemyAttackType atkType = enemyAttackPattern[enemyPatternIndex];
        enemyPatternIndex = (enemyPatternIndex + 1) % enemyAttackPattern.Count;

        int attackPos = GetAttackPosition();
        SpriteRenderer atkSprite = GetAttackSprite(attackPos);


        // right after you get atkSprite and atkType:
        if (atkSprite != null)
        {
            switch (atkType)
            {
                case EnemyAttackType.Dodge:
                    shieldManager.FlashDodgeIndicator(atkSprite);
                    break;
                case EnemyAttackType.Shield:
                    shieldManager.FlashAttackIndicator(atkSprite);
                    break;
                case EnemyAttackType.Parry:
                    shieldManager.FlashCrouchIndicator(atkSprite);
                    break;
            }
        }



        // determine current wind-up duration
        float currentWindUp = TimingController.Instance.SkillPhase
            ? windUpTime = .25f   // half as long in Skill-Phase
            : windUpTime;         // normal otherwise

        // WIND‑UP
        SetAnimationState("WindUp");
        WindUpSound();
        yield return new WaitForSeconds(windUpTime);

        // PARRY WINDOW
        bool didParry = false;
        
        
            parryWindowActive = true;
            float t = 0f;
            while (t < parryWindow)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // successful parry → bonus time, no shield movement
                    //TimingController.Instance.AddTime(parryBonusTime);
                    didParry = true;
                    break;
                }
                t += Time.deltaTime;
                yield return null;
            }
        parryWindowActive = false;
        

        // RESOLVE ATTACK
        ResolveAttack(attackPos, atkType, didParry);
        yield return new WaitForSeconds(0.2f);
        CleanupAttack(atkSprite, attackPos);
        EnemyAttackQueue.AttackFinished(this);
    }


    /// <summary>
    /// Advance the attack‐sequence by the given number of steps.
    /// If that jumps past the end, the enemy dies.
    /// Otherwise immediately refreshes both the main indicator and the next‐hit icon (and health bar).
    /// </summary>
    public void SkipPattern(int steps)
    {
        currentSequenceIndex += steps;

        // if we hit or pass the end of the sequence, kill the enemy
        if (currentSequenceIndex >= attackSequence.Count)
        {
            Die();
            return;
        }

        // refresh the attack indicator + next‐hit reveal
        UpdateColor();                  // updates attackIndicatorRenderer + calls UpdateNextHitIcon()

        // force the health bar to update right away (optional, since your Update loop does it too)
        if (healthBar != null)
            healthBar.value = attackSequence.Count - currentSequenceIndex;
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
    /// Sets the animation states for wind-up and attack.
    /// </summary>
    private void SetAnimationState(string animationSequence)
    {
        animator.SetTrigger(animationSequence);
    }

    /// <summary>
    /// Determines whether the player dodged successfully and applies the appropriate effects.
    /// </summary>
    protected void ResolveAttack(int attackPos, EnemyAttackType atkType, bool didParry)
    {
        int playerPos = Mathf.RoundToInt(dodgeSlider.value);
        bool shieldBusy = shieldManager.ParryInProgress;
        AttackSound();

        if (atkType == EnemyAttackType.Dodge)
        {
            // if you parried, always succeed
            if (didParry && playerPos == attackPos)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
            }
            // otherwise succeed only if you moved out of the attack position
            else if (playerPos != attackPos && !shieldBusy)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
            }
            else
            {
                // take damage
                int damageAmount = 1;
                var fc = FindObjectOfType<FightController>();
                if (fc != null && fc.invincibleHits > 0)
                {
                    fc.invincibleHits--;
                    damageAmount = 0;
                }
                EventManager.Instance.TriggerEvent("takeDamageEvent", damageAmount);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHit, transform.position);
            }
        }
        else if(atkType == EnemyAttackType.Shield) 
        {
            if (didParry && playerPos == attackPos)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
            }
            else if(playerPos == attackPos && !shieldBusy)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
                shieldManager?.TriggerShieldRecoil(attackPos, this);
            }
            else
            {
                // determine damage based on invincibility charges
                int damageAmount = 1;
                var fc = FindObjectOfType<FightController>();
                if (fc != null && fc.invincibleHits > 0)
                {
                    fc.invincibleHits--;
                    damageAmount = 0;
                    Debug.Log($"Invincible! Charges left: {fc.invincibleHits}");
                }

                EventManager.Instance.TriggerEvent("takeDamageEvent", damageAmount);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHit, transform.position);
            }
        }
        else if (atkType == EnemyAttackType.Parry)
        {
            if (didParry && !shieldBusy && playerPos == attackPos) // only a Space‑parry will block
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);
            }
            else
            {
                // take damage as before
                int damageAmount = 1;
                var fc = FindObjectOfType<FightController>();
                if (fc != null && fc.invincibleHits > 0)
                {
                    fc.invincibleHits--;
                    damageAmount = 0;
                }
                EventManager.Instance.TriggerEvent("takeDamageEvent", damageAmount);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHit, transform.position);
            }
        }


        // Play attack animation and sound
        if (gameObject.tag == "Frog" && attackPos == 0)
        {
            animator.SetTrigger("AttackLeft");

        }
        else if (gameObject.tag == "Frog" && attackPos == 1)
        {
            animator.SetTrigger("AttackMiddle");
        }
        else if (gameObject.tag == "Frog" && attackPos == 2)
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
        else if (this.gameObject.tag == "Cleric")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.CultistWU, transform.position);
        }
        else if (this.gameObject.tag == "Devil")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.demonWU, transform.position);
        }
        else if (this.gameObject.tag == "Vampire")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.vampireWU, transform.position);
        }
        else if (this.gameObject.tag == "Collector")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.collectorWU, transform.position);
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
        else if (this.gameObject.tag == "Cleric")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.cultistAtk, transform.position);
        }
        else if (this.gameObject.tag == "Devil")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.demonAtk, transform.position);
        }
        else if (this.gameObject.tag == "Vampire")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.vampireAtk, transform.position);
        }
        else if (this.gameObject.tag == "Collector")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.collectorAttack, transform.position);
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

        UpdateNextHitIcon();
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



            if (this.gameObject.tag == "Knight")
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.knightDamage, this.transform.position);
            }
            if (this.gameObject.tag == "Collector")
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.collectorDamage, this.transform.position);
            }
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

        shieldManager?.CancelAllShieldEffects();

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

        if (isAttacking)
            EnemyAttackQueue.AttackFinished(this);

        ResourceManager.Instance.AddResource("enemiesKilled", 1);

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
    protected virtual void SetNewParent(Transform newParent)
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


    /// <summary>
    /// slowFactor < 1.0 slows everything down (anim & attack cadence)
    /// </summary>
    public void SetSlow(float slowFactor)
    {
        if (isSlowed) return;
        isSlowed = true;

        // stretch out attack timing
        attackIntervalMin = normalIntervalMin / slowFactor;
        attackIntervalMax = normalIntervalMax / slowFactor;
        windUpTime = normalWindUpTime / slowFactor;

        // slow animator playback
        if (animator != null)
            animator.speed = slowFactor;
    }

    public void ResetSpeed()
    {
        if (!isSlowed) return;
        isSlowed = false;

        attackIntervalMin = normalIntervalMin;
        attackIntervalMax = normalIntervalMax;
        windUpTime = normalWindUpTime;

        if (animator != null)
            animator.speed = 1f;
    }

}

