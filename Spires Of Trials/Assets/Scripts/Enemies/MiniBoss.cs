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
    private float movementSpeed = 10f;
    private float windUpRiseDistance = 1f;
    private float attackDropDistance = 1f;


    private Animator animator;

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

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoop());

        animator = this.GetComponent<Animator>();
    }

    private void UpdateColor()
    {
        if (currentSequenceIndex >= attackSequence.Count) return;

        string nextAttack = attackSequence[currentSequenceIndex];
        switch (nextAttack)
        {
            case "melee":
                spriteRenderer.color = meleeColor;
                break;
            case "magic":
                spriteRenderer.color = magicColor;
                break;
            case "range":
                spriteRenderer.color = rangeColor;
                break;
            case "heavy":
                spriteRenderer.color = heavyColor;
                break;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {

           

            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);
            isAttacking = true;

            // Get the player's current dodge position
            // Move MiniBoss to a random spawn point before attacking
            Transform randomSpawn = GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn);
            SetNewParent(randomSpawn);

            animator.SetBool("IsWinding", true);

            yield return new WaitForSeconds(0.3f); // Small delay before attacking

            animator.SetBool("IsWinding", false);
            animator.SetBool("IsAttacking", true);

            // Get the player's current dodge position
            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
            int miniBossTargetPos = 0;

            if(playerDodgePosition == 0)
            {
                miniBossTargetPos = Random.Range(1, 3);

            }else if(playerDodgePosition == 1)
            {
                miniBossTargetPos = Random.Range(0, 2) * 2;
            }
            else
            {
                miniBossTargetPos = Random.Range(0, 2);
            }

            Transform targetPosition = GetSpawnFromIndex(miniBossTargetPos);

            if (targetPosition != null)
            {
                //SetNewParent(targetPosition);
            }


            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.HighlightPosition(miniBossTargetPos);
            }

            yield return new WaitForSeconds(windUpTime); // Wind-up delay before attack

            // Attack drop-down effect
           // yield return MoveEnemy(transform.position - Vector3.up * attackDropDistance, movementSpeed);

            // Check if the player is still in the same position
            // Check if the player is still in the same position
            int updatedPlayerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
            if (updatedPlayerDodgePosition == miniBossTargetPos)
            {
                Debug.Log("Player successfully blocked the MiniBoss attack!");
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
            // UpdateColor();

            yield return new WaitForSeconds(.1f);

            isAttacking = false;
            animator.SetBool("IsAttacking", false);
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
