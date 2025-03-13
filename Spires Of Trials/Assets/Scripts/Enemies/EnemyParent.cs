using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 3;
    protected int currentHealth;

    protected Slider dodgeSlider;
    protected DodgeBarHighlighter dodgeBarHighlighter;

     protected Color normalColor = Color.white;
    [SerializeField] protected Color windUpColor = Color.blue;
    [SerializeField] protected Color attackColor = Color.magenta;
    [SerializeField] protected Color damageColor = Color.red;

    protected Coroutine flashCoroutine;

    //private Coroutine finalFlashCoroutine; // Track final flash separately

    [SerializeField] protected Slider healthBar;
    protected Transform healthBarTransform;
    [SerializeField] protected Image healthBarFill; // Assign the Fill Image in Inspector
    [SerializeField] protected Gradient healthGradient; // Create a Gradient in Inspector




    protected float flashDuration = 0.2f;



    protected Color meleeColor = Color.red;
    protected Color magicColor = Color.blue;
    protected Color rangeColor = Color.green;
    protected Color heavyColor = Color.yellow;

    protected float attackIntervalMin = .5f;
    protected float attackIntervalMax = 2f;
    protected float windUpTime = 1f;
    //private float attackDropDistance = 1f;
    //private float windUpRiseDistance = 0.5f;
    //private float movementSpeed = 10f;

    protected Vector3 originalPosition;
    protected SpriteRenderer spriteRenderer;
    protected Coroutine attackCoroutine;
    protected bool isAttacking = false;

    protected int enemyAttackPosition;
    protected List<string> attackSequence = new List<string>();
    protected int currentSequenceIndex = 0;

    protected Animator animator;

    [SerializeField] protected GameObject attackIndicator; // Assign in Inspector (e.g., an empty GameObject with a SpriteRenderer)
    [SerializeField] protected Sprite meleeSprite;
    [SerializeField] protected Sprite magicSprite;
    [SerializeField] protected Sprite rangeSprite;
    [SerializeField] protected Sprite heavySprite;
    protected SpriteRenderer attackIndicatorRenderer;


    protected SpriteRenderer leftAttackSprite;
    protected SpriteRenderer centerAttackSprite;
    protected SpriteRenderer rightAttackSprite;

    [SerializeField] protected GameObject damageParticlePrefab; // Assign the prefab in the Inspector
    [SerializeField] protected GameObject partOrgin;


    protected Transform leftShield;
    protected Transform centerShield;
    protected Transform rightShield;


    protected Coroutine activeRecoilCoroutine;

    private bool isRecoiling = false;



    protected virtual void Start()
    {

     



        

        StartCoroutine(MonitorColorReset()); // Start monitoring color resets



        currentHealth = maxHealth;
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;

        animator = GetComponent<Animator>();

        // Ensure attackIndicator is properly set up
        if (attackIndicator != null)
        {
            attackIndicatorRenderer = attackIndicator.GetComponent<SpriteRenderer>();
            if (attackIndicatorRenderer == null)
            {
                Debug.LogError($"SpriteRenderer missing on {attackIndicator.name}. Please add one.");
            }
        }
        else
        {
            Debug.LogError($"attackIndicator is not assigned for {gameObject.name}. Assign it in the Inspector.");
        }

        DefineAttackSequence();
        UpdateColor();


        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoop());





        healthBar.maxValue = attackSequence.Count;
        healthBar.value = attackSequence.Count; // Start full


    }


    protected void TriggerShieldRecoil(int position)
    {
        Transform shieldToRecoil = GetShieldByPosition(position);

        if (shieldToRecoil != null && shieldToRecoil.gameObject.activeSelf)
        {
            Debug.Log($"Attempting to trigger shield recoil at position {position}");

            if (!isRecoiling) // Check if shield is already recoiling
            {
                isRecoiling = true; // Lock recoil
                activeRecoilCoroutine = StartCoroutine(ShieldRecoil(shieldToRecoil));
            }
        }
    }

    protected IEnumerator ShieldRecoil(Transform shield)
    {
        Vector3 originalPosition = shield.position;
        Vector3 recoilPosition = originalPosition + new Vector3(0, -0.2f, 0);

        Debug.Log($"Recoil Start for {shield.name} at {shield.position}");

        shield.position = recoilPosition; // Move down slightly
        yield return new WaitForSeconds(0.1f); // Short delay

        shield.position = originalPosition; // Reset back
        Debug.Log($"Recoil End for {shield.name}");

        isRecoiling = false; // Unlock recoil after animation
        activeRecoilCoroutine = null; // Clear coroutine reference
    }



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



    public void InitializeAttackSprites(SpriteRenderer left, SpriteRenderer center, SpriteRenderer right, Transform lShield, Transform cShield, Transform rShield)
    {
        leftAttackSprite = left;
        centerAttackSprite = center;
        rightAttackSprite = right;

        leftShield = lShield;
        centerShield = cShield;
        rightShield = rShield;
    }


    private IEnumerator MonitorColorReset()
    {
        SpriteRenderer iconRenderer = transform.childCount > 0 ? transform.GetChild(0).GetComponent<SpriteRenderer>() : null;

        while (true)
        {
            yield return new WaitForSeconds(0.5f); // Adjust check frequency as needed

            if (spriteRenderer.color != normalColor && !isAttacking)
            {
                spriteRenderer.color = normalColor;
            }

            if (iconRenderer != null && iconRenderer.color != normalColor && !isAttacking)
            {
                iconRenderer.color = normalColor;
            }
        }
    }




    protected void Update()
    {
        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex;
            float healthPercentage = healthBar.value / healthBar.maxValue;
            healthBarFill.color = healthGradient.Evaluate(healthPercentage);
        }

        // **Check if stuck in attack animation and reset**
        if (isAttacking && !animator.GetBool("IsAttacking"))
        {
            isAttacking = false;
        }
    }




    protected void DefineAttackSequence()
    {
        switch (gameObject.tag)
        {
            case "Skeleton":
                attackSequence = new List<string> { "melee", "heavy", "range", "magic" };
                break;
            case "Goblin":
                attackSequence = new List<string> { "magic", "range", "heavy", "melee" };
                break;
            case "Slime":
                attackSequence = new List<string> { "heavy", "magic", "melee", "range" };
                break;
            case "Knight":
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
                break;
            default:
                attackSequence = new List<string> { "melee" };
                break;
        }
    }

    private void UpdateColor()
    {
        if (currentSequenceIndex >= attackSequence.Count) return;
        if (attackIndicatorRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: attackIndicatorRenderer is NULL. Ensure attackIndicator has a SpriteRenderer.");
            return;
        }

        string nextAttack = attackSequence[currentSequenceIndex];
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

    protected IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;

        // **Ensure the sprite is visible before flashing**
        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;

        // Stop any existing flash coroutine
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Start the flash effect
        flashCoroutine = StartCoroutine(FlashRoutine(attackSprite));
    }


    protected IEnumerator FlashRoutine(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;

        Color originalColor = attackSprite.color;

        // **Ensure it’s enabled before flashing**
        attackSprite.enabled = true;

        for (int i = 0; i < 3; i++) // Flash 3 times
        {
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.85f); // Slightly transparent
            yield return new WaitForSeconds(0.15f);
            attackSprite.color = originalColor; // Reset
            yield return new WaitForSeconds(0.15f);
        }

        // **Keep it enabled for the next attack**
        attackSprite.enabled = true;

        flashCoroutine = null;
    }




    protected virtual int GetAttackPosition()
    {
        return enemyAttackPosition;
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            float waitTime = Mathf.Round(Random.Range(attackIntervalMin, attackIntervalMax) * 10f) / 10f;
            Debug.Log($"Next attack in {waitTime} seconds"); // Debugging info

            yield return new WaitForSeconds(waitTime);

            // Request to attack
            EnemyAttackQueue.RequestAttack(this);

            // Wait before restarting the loop (ensuring waitTime applies to every cycle)
            yield return new WaitForSeconds(waitTime);
        }
    }


    public void StartAttack()
    {
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        int attackPosition = GetAttackPosition();
        SpriteRenderer attackSprite = null;

        if (attackPosition == 0) attackSprite = leftAttackSprite;
        else if (attackPosition == 1) attackSprite = centerAttackSprite;
        else if (attackPosition == 2) attackSprite = rightAttackSprite;

        

     

        if (gameObject.tag == "Goblin")
        {
            spriteRenderer.flipX = attackPosition == 2;
        }

        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.HighlightPosition(attackPosition);

        if (attackSprite != null)
        {
            Debug.Log($"Starting Flash for {attackSprite.gameObject.name}");

            // **Force enable before flashing**
            attackSprite.gameObject.SetActive(true);
            attackSprite.enabled = true;

            StartCoroutine(FlashAttackIndicator(attackSprite));
        }
        else
        {
            Debug.LogError("Attack Sprite is NULL!");
        }


        // Ensure the animation starts correctly
        animator.SetBool("IsWinding", true);
        animator.SetBool("IsAttacking", false); // Ensure it's false before the attack

        WindUpSound();

        yield return new WaitForSeconds(windUpTime);



        // Make sure the wind-up animation stops and attack animation starts
        animator.SetBool("IsWinding", false);
       

        AttackSound();

        int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

        animator.SetBool("IsAttacking", true); // Explicitly setting attack animation here
        if (playerDodgePosition == attackPosition)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldWood, transform.position);

            if (activeRecoilCoroutine != null) StopCoroutine(activeRecoilCoroutine);
            TriggerShieldRecoil(attackPosition);
        }          
        else
        {
            Debug.Log("Player failed to block! Taking damage.");
            EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerMetal, transform.position);
        }

        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.ClearHighlight(attackPosition);


        

        yield return new WaitForSeconds(0.2f); // Give time for the attack animation to play

        isAttacking = false;
        animator.SetBool("IsAttacking", false); // Explicitly resetting attack animation

        // Ensure attack indicator is turned off after the attack ends
        if (attackSprite != null)
            attackSprite.enabled = false; // This guarantees that the sprite is turned off



        // Ensure attack indicator is turned off after the attack ends
        if (attackSprite != null)
        {
            attackSprite.enabled = false; // Guarantees it is turned off

            // Stop flashing if still running
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
        }

        // Notify manager that the attack finished
        EnemyAttackQueue.AttackFinished(this);


        // Ensure the animation resets after attack
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsWinding", false);
        isAttacking = false;


    }


    void WindUpSound()
    {
        if(this.gameObject.tag == "Goblin")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.gobWU, transform.position);
        }
        else if(this.gameObject.tag == "Skeleton")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.skeWU, transform.position);
        }
        else if (this.gameObject.tag == "Slime")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.slimeWU, transform.position);
        }
    }

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

    private IEnumerator MoveEnemy(Vector3 targetPosition, float speed)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }

    public void TakeDamage(string attackType)
    {
        PlayerAttackingScript player = FindObjectOfType<PlayerAttackingScript>(); // Find the player script

        if (currentSequenceIndex < attackSequence.Count && attackType == attackSequence[currentSequenceIndex])
        {
            currentSequenceIndex++;
            Debug.Log($"{gameObject.name} hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            StartCoroutine(FlashRed()); // Trigger red flash

            if (damageParticlePrefab != null)
            {

                GameObject particles = Instantiate(damageParticlePrefab, partOrgin.transform.position, Quaternion.identity);
                Destroy(particles, 0.5f); // Cleanup after 0.5 sec

            }


            player?.UpdateCombo(true); // **Notify the player about a successful hit**

            if (currentSequenceIndex >= attackSequence.Count)
            {
                Die();
            }
            else
            {
                UpdateColor();
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} hit incorrectly! Resetting sequence.");
            currentSequenceIndex = 0;

            if (healthBar != null)
                healthBar.value = 0; // Reset on incorrect hit

            player?.UpdateCombo(false); // **Notify the player about a failed hit (miss)**

            UpdateColor();
        }
    }


    protected IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            SpriteRenderer iconRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>(); // Assumes the first child is the icon

            Color iconOriginalColor = iconRenderer != null ? iconRenderer.color : Color.white;

            // Flash red
            spriteRenderer.color = Color.red;
            if (iconRenderer != null)
                iconRenderer.color = Color.red;

            yield return new WaitForSeconds(0.2f);

            // Ensure it returns to the correct color after flashing
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            if (iconRenderer != null)
                iconRenderer.color = iconOriginalColor;
        }
    }





    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Ensure the attack indicator is turned off before destruction
        if (attackIndicatorRenderer != null)
        {
            attackIndicatorRenderer.enabled = false;
        }

        //// Stop the final flash if it's still running
        //if (finalFlashCoroutine != null)
        //{
        //    StopCoroutine(finalFlashCoroutine);
        //    finalFlashCoroutine = null;
        //}

        // Ensure attack sprites are disabled if they were being used
        if (leftAttackSprite != null)
            leftAttackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);

        if (centerAttackSprite != null)
            centerAttackSprite.color = new Color(centerAttackSprite.color.r, centerAttackSprite.color.g, centerAttackSprite.color.b, 0f);

        if (rightAttackSprite != null)
            rightAttackSprite.color = new Color(rightAttackSprite.color.r, rightAttackSprite.color.g, rightAttackSprite.color.b, 0f);


        // Ensure any attack flash is stopped
        //StopAllCoroutines();

        // Ensure health bar is destroyed
        if (healthBar != null)
            Destroy(this.healthBar.gameObject);

        // Ensure the highlight is cleared before destroying
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

        // Stop this enemy's flashing effect
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }



        // Notify manager that this enemy is no longer attacking
        EnemyAttackQueue.AttackFinished(this);

        // Destroy the enemy game object
        Destroy(gameObject);

        // Stop the final flash if it's still running
        //if (finalFlashCoroutine != null)
        //{
        //    StopCoroutine(finalFlashCoroutine);
        //    finalFlashCoroutine = null;
        //}

    }



    protected void OnDisable()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Ensure the highlight and flash are cleared
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }


        // Ensure attack indicator is turned off when the enemy is disabled
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }


        // Ensure attack indicator is turned off when the enemy is disabled
        if (leftAttackSprite != null)
            leftAttackSprite.color = new Color(leftAttackSprite.color.r, leftAttackSprite.color.g, leftAttackSprite.color.b, 0f);

        if (centerAttackSprite != null)
            centerAttackSprite.color = new Color(centerAttackSprite.color.r, centerAttackSprite.color.g, centerAttackSprite.color.b, 0f);

        if (rightAttackSprite != null)
            rightAttackSprite.color = new Color(rightAttackSprite.color.r, rightAttackSprite.color.g, rightAttackSprite.color.b, 0f);


        //StopAllCoroutines(); // Ensures no lingering effects
    }






}
