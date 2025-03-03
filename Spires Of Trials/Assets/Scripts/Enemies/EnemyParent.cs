using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private Slider dodgeSlider;
    private DodgeBarHighlighter dodgeBarHighlighter;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color windUpColor = Color.blue;
    [SerializeField] private Color attackColor = Color.magenta;
    [SerializeField] private Color damageColor = Color.red;

    
    [SerializeField] Slider healthBar;
    private Transform healthBarTransform;
    [SerializeField] private Image healthBarFill; // Assign the Fill Image in Inspector
    [SerializeField] private Gradient healthGradient; // Create a Gradient in Inspector




    private float flashDuration = 0.2f;



    private Color meleeColor = Color.red;
    private Color magicColor = Color.blue;
    private Color rangeColor = Color.green;
    private Color heavyColor = Color.yellow;

    private float attackIntervalMin = 2f;
    private float attackIntervalMax = 2.5f;
    private float windUpTime = 1f;
    //private float attackDropDistance = 1f;
    //private float windUpRiseDistance = 0.5f;
    //private float movementSpeed = 10f;

    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    protected int enemyAttackPosition;
    private List<string> attackSequence = new List<string>();
    private int currentSequenceIndex = 0;

    private Animator animator;

    [SerializeField] private GameObject attackIndicator; // Assign in Inspector (e.g., an empty GameObject with a SpriteRenderer)
    [SerializeField] private Sprite meleeSprite;
    [SerializeField] private Sprite magicSprite;
    [SerializeField] private Sprite rangeSprite;
    [SerializeField] private Sprite heavySprite;
    private SpriteRenderer attackIndicatorRenderer;



   private SpriteRenderer leftAttackSprite;
   private SpriteRenderer centerAttackSprite;
   private SpriteRenderer rightAttackSprite;

   

    protected virtual void Start()
    {
       
        GameObject leftObj = GameObject.FindGameObjectWithTag("LeftFlash");
        GameObject centerObj = GameObject.FindGameObjectWithTag("MiddleFlash");
        GameObject rightObj = GameObject.FindGameObjectWithTag("RightFlash");

            

        leftAttackSprite = leftObj != null ? leftObj.GetComponent<SpriteRenderer>() : null;
        centerAttackSprite = centerObj != null ? centerObj.GetComponent<SpriteRenderer>() : null;
        rightAttackSprite = rightObj != null ? rightObj.GetComponent<SpriteRenderer>() : null;

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

        attackCoroutine = StartCoroutine(AttackLoop());





        healthBar.maxValue = attackSequence.Count;
        healthBar.value = attackSequence.Count; // Start full


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




    private void Update()
    {
        if (healthBar != null)
        {
            healthBar.value = attackSequence.Count - currentSequenceIndex; // Update health value

            // Apply gradient based on current health
            float healthPercentage = healthBar.value / healthBar.maxValue;
            healthBarFill.color = healthGradient.Evaluate(healthPercentage);
        }
    }



    private void DefineAttackSequence()
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

    private IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;


        Debug.LogWarning("wtf");

        Color originalColor = attackSprite.color;
        attackSprite.enabled = true; // Ensure it's visible

        for (int i = 0; i < 3; i++) // Flash 3 times
        {
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.85f); // Set opacity to 0.5
            yield return new WaitForSeconds(0.15f);
            attackSprite.color = originalColor; // Reset to original color
            yield return new WaitForSeconds(0.15f);
        }

        attackSprite.enabled = false; // Hide after flashing
    }





    protected virtual int GetAttackPosition()
    {
        return enemyAttackPosition;
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Request to attack
            EnemyAttackQueue.RequestAttack(this);
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
        animator.SetBool("IsAttacking", true); // Explicitly setting attack animation here

        AttackSound();

        int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
        if (playerDodgePosition == attackPosition)
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldBlock, transform.position);
        else
        {
            Debug.Log("Player failed to block! Taking damage.");
            EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.playerHit, transform.position);
        }

        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.ClearHighlight(attackPosition);

        yield return new WaitForSeconds(0.2f); // Give time for the attack animation to play

        isAttacking = false;
        animator.SetBool("IsAttacking", false); // Explicitly resetting attack animation

        // Ensure attack indicator is turned off after the attack ends
        if (attackSprite != null)
            attackSprite.enabled = false; // This guarantees that the sprite is turned off

        // **Final bright flash effect before attack lands**
        if (attackSprite != null)
            StartCoroutine(FinalBrightFlash(attackSprite));

        // Notify manager that the attack finished
        EnemyAttackQueue.AttackFinished(this);
    }

    private IEnumerator FinalBrightFlash(SpriteRenderer attackSprite)
    {
        if (attackSprite == null) yield break;

        Color originalColor = attackSprite.color;

        attackSprite.enabled = true; // Ensure it's visible

        // Bright flash effect (full opacity, bright white tint)
        attackSprite.color = new Color(originalColor.r, originalColor.b, originalColor.g, 1f); // White and fully opaque
        yield return new WaitForSeconds(0.1f);

        // Return to original color
        attackSprite.color = originalColor;
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
        if (currentSequenceIndex < attackSequence.Count && attackType == attackSequence[currentSequenceIndex])
        {
            currentSequenceIndex++;
            Debug.Log($"{gameObject.name} hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

            StartCoroutine(FlashRed()); // Trigger red flash

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
            UpdateColor();
        }
    }

    private IEnumerator FlashRed()
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





    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Ensure the attack indicator is turned off before destruction
        if (attackIndicatorRenderer != null)
        {
            attackIndicatorRenderer.enabled = false;
        }

        // Ensure attack sprites are disabled if they were being used
        if (leftAttackSprite != null) leftAttackSprite.enabled = false;
        if (centerAttackSprite != null) centerAttackSprite.enabled = false;
        if (rightAttackSprite != null) rightAttackSprite.enabled = false;

        // Ensure health bar is destroyed
        if (healthBar != null)
            Destroy(this.healthBar.gameObject);

        // Ensure the highlight is cleared before destroying
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

        // Stop any attack coroutine that might still be running
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Notify manager that this enemy is no longer attacking
        EnemyAttackQueue.AttackFinished(this);

        // Destroy the enemy game object
        Destroy(gameObject);
    }



    private void OnDisable()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Ensure the highlight and flash are cleared
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

       
    }

  



}
