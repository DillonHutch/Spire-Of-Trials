using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MiniBoss : MonoBehaviour
{
    private enum AttackMode { Red, Blue, Green }
    private AttackMode currentMode;

    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private Slider dodgeSlider;
    private DodgeBarHighlighter dodgeBarHighlighter;

    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color damageColor = Color.magenta;

    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform centerSpawn;
    [SerializeField] private Transform rightSpawn;

    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;

    private Transform currentParent;
    private bool canBeHitByMelee = false;
    private bool canBeHitByMagic = false;
    private bool canBeHitByRange = false;

    private float attackIntervalMin = 1f;
    private float attackIntervalMax = 2f;
    private float windUpTime = 0.5f;

    private bool isAttacking = false;

    private void Start()
    {
        currentHealth = maxHealth;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();
        dodgeBarHighlighter = FindObjectOfType<DodgeBarHighlighter>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ChangeAttackMode(); // Initialize attack mode
        attackCoroutine = StartCoroutine(AttackLoop());
    }

    private void ChangeAttackMode()
    {
        int mode = Random.Range(0, 3);
        currentMode = (AttackMode)mode;

        switch (currentMode)
        {
            case AttackMode.Red:
                spriteRenderer.color = redColor;
                canBeHitByMelee = true;
                canBeHitByMagic = false;
                canBeHitByRange = false;
                SetNewParent(GetRandomSpawn(leftSpawn, centerSpawn, rightSpawn));
                break;

            case AttackMode.Blue:
                spriteRenderer.color = blueColor;
                canBeHitByMelee = false;
                canBeHitByMagic = true;
                canBeHitByRange = false;
                SetNewParent(centerSpawn); // Always in the center
                break;

            case AttackMode.Green:
                spriteRenderer.color = greenColor;
                canBeHitByMelee = false;
                canBeHitByMagic = false;
                canBeHitByRange = true;
                SetNewParent(GetRandomSpawn(leftSpawn, rightSpawn)); // Only left or right
                break;
        }

        Debug.Log($"MiniBoss changed to {currentMode} mode at {currentParent.name}");
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);
            isAttacking = true;

            int attackPosition = GetAttackPosition();

            // Highlight attack position
            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.HighlightPosition(attackPosition);
            }

            yield return new WaitForSeconds(windUpTime);

            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

            if (playerDodgePosition == attackPosition)
            {
                Debug.Log("Player hit by MiniBoss attack!");
                EventManager.Instance.TriggerEvent("takeDamageEvent", 2); // Hits harder
            }
            else
            {
                Debug.Log("Player dodged MiniBoss attack!");
            }

            // Clear highlight
            if (dodgeBarHighlighter != null)
            {
                dodgeBarHighlighter.ClearHighlight(attackPosition);
            }

            isAttacking = false;

            // Change attack mode after each attack
            ChangeAttackMode();
        }
    }

    private int GetAttackPosition()
    {
        switch (currentMode)
        {
            case AttackMode.Red:
                return Mathf.Clamp(GetPositionIndex(currentParent), 0, 2); // Attack position in front

            case AttackMode.Blue:
                return (Random.value > 0.5f) ? 0 : 2; // 50/50 attack pos 0 or 2

            case AttackMode.Green:
                return (GetPositionIndex(currentParent) == 0) ? 2 : 0; // Attack two away

            default:
                return 1;
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
        if (isAttacking)
        {
            Debug.Log("MiniBoss cannot be damaged while attacking.");
            return;
        }

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
        ChangeAttackMode(); // Change mode on damage
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
            transform.SetParent(currentParent);
            transform.localPosition = Vector3.zero; // Ensures correct alignment
        }
    }

    private Transform GetRandomSpawn(params Transform[] positions)
    {
        return positions[Random.Range(0, positions.Length)];
    }

    private int GetPositionIndex(Transform position)
    {
        if (position == leftSpawn) return 0;
        if (position == centerSpawn) return 1;
        if (position == rightSpawn) return 2;
        return 1; // Default to center if unknown
    }
}
