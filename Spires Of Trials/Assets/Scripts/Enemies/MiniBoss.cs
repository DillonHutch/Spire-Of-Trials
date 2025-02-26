using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniBoss : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private Slider dodgeSlider;
    private DodgeBarHighlighter dodgeBarHighlighter;

    private Color meleeColor = Color.red;
    private Color magicColor = Color.blue;
    private Color rangeColor = Color.green;
    private Color heavyColor = Color.yellow;

    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform centerSpawn;
    [SerializeField] private Transform rightSpawn;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;
    private Transform currentParent;
    private bool isAttacking = false;

    private float attackIntervalMin = 1f;
    private float attackIntervalMax = 1f;
    private float windUpTime = .6f;
    //private float movementSpeed = 10f;
    //private float windUpRiseDistance = 1f;
    //private float attackDropDistance = 1f;


    private Animator animator;

    [SerializeField] private GameObject attackIndicator; // Assign in Inspector
    [SerializeField] private Sprite meleeSprite;
    [SerializeField] private Sprite magicSprite;
    [SerializeField] private Sprite rangeSprite;
    [SerializeField] private Sprite heavySprite;
    private SpriteRenderer attackIndicatorRenderer;

    private Image leftFlash;
    private Image middleFlash;
    private Image rightFlash;
    private float flashDuration = 0.2f;


    private List<string> attackSequence = new List<string>
    {
        "melee", "magic", "range", "heavy",
        "magic", "melee", "range", "heavy", "melee", "magic",
        "range", "heavy", "melee", "range", "magic", "heavy",
        "melee", "magic", "range", "heavy", "magic", "melee",
        "range", "heavy", "melee", "magic", "range", "heavy",
        "magic", "melee", "range", "heavy"
    };

    private int currentSequenceIndex = 0;
    private Vector3 originalPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {

        leftFlash = GameObject.FindGameObjectWithTag("LeftFlash")?.GetComponent<Image>();
        middleFlash = GameObject.FindGameObjectWithTag("MiddleFlash")?.GetComponent<Image>();
        rightFlash = GameObject.FindGameObjectWithTag("RightFlash")?.GetComponent<Image>();

        if (leftFlash == null || middleFlash == null || rightFlash == null)
        {
            Debug.LogError("One or more flash UI elements not found! Ensure they have the correct tags.");
        }


        currentHealth = maxHealth;
        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider")?.GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        leftSpawn = GameObject.FindWithTag("LeftSpawn")?.transform;
        centerSpawn = GameObject.FindWithTag("MiddleSpawn")?.transform;
        rightSpawn = GameObject.FindWithTag("RightSpawn")?.transform;

        if (leftSpawn == null || centerSpawn == null || rightSpawn == null)
        {
            Debug.LogError("MiniBoss spawn positions are not properly set! Check your tags.");
            return;
        }

        Debug.Log("MiniBoss has spawned and initialized spawn positions.");

        SetNewParent(GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn));
        UpdateColor();

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



        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoop());

        animator = this.GetComponent<Animator>();

        UpdateColor();
    }

    private void UpdateColor()
    {
        if (currentSequenceIndex >= attackSequence.Count) return;
        if (attackIndicatorRenderer == null)
        {
           // Debug.LogError($"{gameObject.name}: attackIndicatorRenderer is NULL. Ensure attackIndicator has a SpriteRenderer.");
            return;
        }

        string nextAttack = attackSequence[currentSequenceIndex];
        switch (nextAttack)
        {
            case "melee":
                attackIndicatorRenderer.sprite = meleeSprite;
                animator.SetFloat("AttackType", 0f);
                break;
            case "magic":
                attackIndicatorRenderer.sprite = magicSprite;
                animator.SetFloat("AttackType", .33f);
                break;
            case "range":
                attackIndicatorRenderer.sprite = rangeSprite;
                animator.SetFloat("AttackType", .66f);
                break;
            case "heavy":
                attackIndicatorRenderer.sprite = heavySprite;
                animator.SetFloat("AttackType", 1f);
                break;
        }
    }


    private IEnumerator AttackLoop()
    {
        while (true)
        {
            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);
            isAttacking = true;

            // Move MiniBoss to a random spawn point before attacking
            Transform randomSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
            SetNewParent(randomSpawn);
            animator.SetTrigger("WindUp");

            int miniBossTargetPos = 0;
            if (playerDodgePosition == 0)
            {
                miniBossTargetPos = Random.Range(1, 3);
            }
            else if (playerDodgePosition == 1)
            {
                miniBossTargetPos = Random.Range(0, 2) * 2;
            }
            else
            {
                miniBossTargetPos = Random.Range(0, 2);
            }

            StartCoroutine(BlinkFlash(miniBossTargetPos, windUpTime));

            yield return new WaitForSeconds(windUpTime - 0.15f); // Almost the full wind-up duration

         

            // Trigger attack AFTER Last Flash has completed
            animator.SetTrigger("Attack");

            // Ensure the Last Flash happens BEFORE Attack
            yield return StartCoroutine(LastFlash(miniBossTargetPos, 0.15f));

            // Now check if the player successfully blocked
            int updatedPlayerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
            if (updatedPlayerDodgePosition == miniBossTargetPos)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.shieldBlock, this.transform.position);
            }
            else
            {
                Debug.Log("Player failed to block! Taking damage from MiniBoss.");
                EventManager.Instance.TriggerEvent("takeDamageEvent", 2);
            }

            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.ClearHighlight(miniBossTargetPos);
            }

            isAttacking = false;
            UpdateColor();

            yield return new WaitForSeconds(.1f);

            isAttacking = false;
            animator.SetTrigger("ReturnToIdle");
        }
    }



    private IEnumerator LastFlash(int attackPosition, float duration)
    {
        Image flashPanel = null;
        switch (attackPosition)
        {
            case 0: flashPanel = leftFlash; break;
            case 1: flashPanel = middleFlash; break;
            case 2: flashPanel = rightFlash; break;
        }

        if (flashPanel == null) yield break;

        // Strong final flash effect
        flashPanel.color = new Color(flashPanel.color.r, flashPanel.color.g, flashPanel.color.b, 0.6f); // Brighter flash
        yield return new WaitForSeconds(duration);
        flashPanel.color = new Color(flashPanel.color.r, flashPanel.color.g, flashPanel.color.b, 0);
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

    private IEnumerator FlashEffect(Image panel)
    {
        Color originalColor = panel.color;
        panel.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f); // Half opacity
        yield return new WaitForSeconds(flashDuration);
        panel.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0); // Reset transparency
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




    private Transform GetSpawnFromIndex(int index)
    {
        switch (index)
        {
            case 0: return leftSpawn;
            case 1: return centerSpawn;
            case 2: return rightSpawn;
            default: return centerSpawn;
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
            Debug.Log($"MiniBoss hit correctly! Progress: {currentSequenceIndex}/{attackSequence.Count}");

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
            Debug.Log("MiniBoss hit incorrectly! Resetting sequence.");
            currentSequenceIndex = 0;
            UpdateColor();
        }
    }

    private void Die()
    {
        Debug.Log("MiniBoss defeated!");
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
    }

    private void SetNewParent(Transform newParent)
    {
        if (newParent != null)
        {
            currentParent = newParent;
            transform.position = currentParent.position;
            transform.SetParent(currentParent);
        }
    }

    private Transform GetRandomSpawn(params Transform[] positions)
    {
        return positions.Length > 0 ? positions[Random.Range(0, positions.Length)] : null;
    }

    private Transform GetPlayerPosition()
    {
        return player.position.x < centerSpawn.position.x ? leftSpawn : (player.position.x > centerSpawn.position.x ? rightSpawn : centerSpawn);
    }

    private int GetAttackPosition()
    {
        return GetPositionIndex(currentParent);
    }

    private int GetPositionIndex(Transform position)
    {
        if (position == leftSpawn) return 0;
        if (position == centerSpawn) return 1;
        if (position == rightSpawn) return 2;
        return 1;
    }
}
