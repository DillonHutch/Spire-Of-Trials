using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private Slider dodgeSlider;
    private DodgeBarHighlighter dodgeBarHighlighter; // Reference to the highlighter
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color windUpColor = Color.blue;
    [SerializeField] private Color attackColor = Color.magenta;
    [SerializeField] private Color damageColor = Color.red;

    private float attackIntervalMin = .1f;
    private float attackIntervalMax = .5f;
    private float windUpTime = .5f;

    [SerializeField] bool canBeHitByMelee = true;
    [SerializeField] bool canBeHitByMagic = false;
    [SerializeField] bool canBeHitByRange = false;

    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;

    private bool isAttacking = false;

    protected int enemyAttackPosition;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>(); // Get the highlighter

        spriteRenderer = GetComponent<SpriteRenderer>();

        attackCoroutine = StartCoroutine(AttackLoop());
    }

    protected virtual int GetAttackPosition()
    {
        return enemyAttackPosition; // Default behavior: Attack its own position
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);
            isAttacking = true;

            int attackPosition = GetAttackPosition();

            // **Highlight attack position**
            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.HighlightPosition(attackPosition);
            }

            spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            spriteRenderer.color = attackColor;
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

            // **Clear only this enemy's highlight**
            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.ClearHighlight(attackPosition);
            }

            spriteRenderer.color = normalColor;
            isAttacking = false;
        }
    }

    public bool CanBeHitBy(string attackType)
    {
        switch (attackType)
        {
            case "melee": return canBeHitByMelee;
            case "magic": return canBeHitByMagic;
            case "range": return canBeHitByRange;
            default: return false;
        }
    }

    public void TakeDamage()
    {
        //if (isAttacking)
        //{
        //    Debug.Log("Enemy cannot be damaged while attacking.");
        //    return;
        //}

        currentHealth -= 1;
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = normalColor;
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
