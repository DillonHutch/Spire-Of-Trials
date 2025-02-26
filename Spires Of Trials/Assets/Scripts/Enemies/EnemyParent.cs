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

 

    private float flashDuration = 0.2f;



    private Color meleeColor = Color.red;
    private Color magicColor = Color.blue;
    private Color rangeColor = Color.green;
    private Color heavyColor = Color.yellow;

    private float attackIntervalMin = 2f;
    private float attackIntervalMax = 2.5f;
    private float windUpTime = .5f;
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

  


    Image leftFlash;
    Image rightFlash;
    Image middleFlash;

    protected virtual void Start()
    {

        leftFlash = GameObject.FindGameObjectWithTag("LeftFlash")?.GetComponent<Image>();
        middleFlash = GameObject.FindGameObjectWithTag("MiddleFlash")?.GetComponent<Image>();
        rightFlash = GameObject.FindGameObjectWithTag("RightFlash")?.GetComponent<Image>();

        if (leftFlash == null || middleFlash == null || rightFlash == null)
        {
            Debug.LogError("One or more flash UI elements not found! Ensure they have the correct tags.");
        }

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
    }


    private void DefineAttackSequence()
    {
        switch (gameObject.tag)
        {
            case "Skeleton":
                attackSequence = new List<string> { "melee", "range", "heavy", "melee" };
                break;
            case "Goblin":
                attackSequence = new List<string> { "magic", "range", "heavy", "melee" };
                break;
            case "Slime":
                attackSequence = new List<string> { "range", "heavy", "melee", "magic" };
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
        FlashScreen(attackPosition);

        if (gameObject.tag == "Goblin")
        {
            if (attackPosition == 2)
               spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
        }

        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.HighlightPosition(attackPosition);

        animator.SetBool("IsWinding", true);
        StartCoroutine(BlinkFlash(attackPosition, windUpTime));
        yield return new WaitForSeconds(windUpTime);

        animator.SetBool("IsWinding", false);
        animator.SetBool("IsAttacking", true);

        int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
        if (playerDodgePosition == attackPosition)
            AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldBlock, this.transform.position);
        else
        {
            Debug.Log("Player failed to block! Taking damage.");
            EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
        }

        if (dodgeBarHighlighter != null)
            dodgeBarHighlighter.ClearHighlight(attackPosition);

        yield return new WaitForSeconds(.1f);

        isAttacking = false;
        animator.SetBool("IsAttacking", false);

        // Notify manager that the attack finished
        EnemyAttackQueue.AttackFinished(this);
    }




    private IEnumerator BlinkFlash(int attackPosition, float duration)
    {
        Image flashPanel = null;
        switch (attackPosition)
        {
            case 0: flashPanel = leftFlash; break;
            case 1: flashPanel = middleFlash; break;
            case 2: flashPanel = rightFlash; break;
        }

        if (flashPanel == null) yield break;

        float blinkInterval = 0.2f; // Adjust for faster/slower blinking
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            flashPanel.color = new Color(flashPanel.color.r, flashPanel.color.g, flashPanel.color.b, 0.2f);
            yield return new WaitForSeconds(blinkInterval);
            flashPanel.color = new Color(flashPanel.color.r, flashPanel.color.g, flashPanel.color.b, 0);
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval * 2;
        }
    }


    private void FlashScreen(int attackPosition)
    {
        Image flashPanel = null;

        switch (attackPosition)
        {
            case 0: flashPanel = leftFlash; break;
            case 1: flashPanel = middleFlash; break;
            case 2: flashPanel = rightFlash; break;
        }

        if (flashPanel != null)
        {
            StartCoroutine(FlashEffect(flashPanel));
        }
    }

    private IEnumerator FlashEffect(Image panel)
    {
        Color originalColor = panel.color;
        panel.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f); // Half opacity
        yield return new WaitForSeconds(flashDuration);
        panel.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0); // Reset transparency
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
            UpdateColor();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Ensure the highlight is cleared before destroying
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }

        // Ensure the flash is cleared
        ClearFlashScreen();

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Notify manager that this enemy is no longer attacking (if needed)
        EnemyAttackQueue.AttackFinished(this);

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

        ClearFlashScreen();
    }

    private void ClearFlashScreen()
    {
        if (leftFlash != null) leftFlash.color = new Color(leftFlash.color.r, leftFlash.color.g, leftFlash.color.b, 0);
        if (middleFlash != null) middleFlash.color = new Color(middleFlash.color.r, middleFlash.color.g, middleFlash.color.b, 0);
        if (rightFlash != null) rightFlash.color = new Color(rightFlash.color.r, rightFlash.color.g, rightFlash.color.b, 0);
    }



}
