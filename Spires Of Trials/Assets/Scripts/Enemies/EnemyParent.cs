using System.Collections;
using System.Collections.Generic;
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


    private Color meleeColor = Color.red;
    private Color magicColor = Color.blue;
    private Color rangeColor = Color.green;
    private Color heavyColor = Color.yellow;

    private float attackIntervalMin = .1f;
    private float attackIntervalMax = .5f;
    private float windUpTime = .5f;
    private float attackDropDistance = 1f;
    private float windUpRiseDistance = 0.5f;
    private float movementSpeed = 10f;

    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    protected int enemyAttackPosition;
    private List<string> attackSequence = new List<string>();
    private int currentSequenceIndex = 0;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;

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
            case "Zombie":
                attackSequence = new List<string> { "magic", "range", "heavy", "melee" };
                break;
            case "Monster":
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
            isAttacking = true;

            int attackPosition = GetAttackPosition();

            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.HighlightPosition(attackPosition);
            }

            yield return MoveEnemy(transform.position + Vector3.up * windUpRiseDistance, movementSpeed);

            yield return new WaitForSeconds(windUpTime);
            yield return MoveEnemy(transform.position - Vector3.up * attackDropDistance, movementSpeed);

            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);
            if (playerDodgePosition == attackPosition)
            {
                //Debug.Log("Player hit by attack!");
                EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
            }
            else
            {
               // Debug.Log("Player dodged the attack!");
            }

            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.ClearHighlight(attackPosition);
            }

            yield return MoveEnemy(originalPosition, movementSpeed);
            UpdateColor();
            isAttacking = false;
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

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        Destroy(gameObject);
    }


    private void OnDisable()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        // Ensure highlight is cleared when the object is disabled
        if (dodgeBarHighlighter != null)
        {
            dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
        }
    }

}
