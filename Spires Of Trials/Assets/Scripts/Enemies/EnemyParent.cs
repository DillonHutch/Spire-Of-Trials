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

    private float attackIntervalMin = .1f;
    private float attackIntervalMax = .5f;
    private float windUpTime = .5f;

    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    protected int enemyAttackPosition;

    // Attack sequence handling
    private List<string> attackSequence = new List<string>();
    private int currentSequenceIndex = 0; // Track current progress

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();

        spriteRenderer = GetComponent<SpriteRenderer>();

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
                attackSequence = new List<string> { "range", "heavy", "melee", "melee" };
                break;
            default:
                attackSequence = new List<string> { "melee" };
                break;
        }
    }


    private Color heavyColor = Color.yellow; // Define color

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
            case "heavy": // New heavy attack color
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

            //spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            //spriteRenderer.color = attackColor;
            yield return new WaitForSeconds(0.1f);

            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

            if (playerDodgePosition == attackPosition)
            {
                Debug.Log("Player hit by attack!");
                EventManager.Instance.TriggerEvent("takeDamageEvent", 1);
            }
            else
            {
                Debug.Log("Player dodged the attack!");
            }

            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.ClearHighlight(attackPosition);
            }

            UpdateColor();
            isAttacking = false;
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

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        Destroy(gameObject);
        dodgeBarHighlighter.ClearHighlight(GetAttackPosition());
    }

    private void OnDisable()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
    }
}
