using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3; // Maximum health for the enemy
    private int currentHealth;

    private Slider dodgeSlider; // Reference to the player's dodge slider
    [SerializeField] private Color normalColor = Color.white; // Default color
    [SerializeField] private Color windUpColor = Color.blue; // Color for wind-up state
    [SerializeField] private Color attackColor = Color.magenta; // Color for attack state
    [SerializeField] private Color damageColor = Color.red; // Color when taking damage
    private float attackIntervalMin = 1f; // Minimum time between attacks
    private float attackIntervalMax = 2f; // Maximum time between attacks
    private float windUpTime = .5f; // Time the enemy winds up before attacking
    [SerializeField] bool canBeHitByMelee = true;
    [SerializeField] bool canBeHitByMagic = false;

    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;

    private bool isAttacking = false; // Flag to prevent damage during attack phase

    int enemyAttackPosition;

    private void Start()
    {
        currentHealth = maxHealth; // Set the current health to the maximum at the start

        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        dodgeSlider = GameObject.FindGameObjectWithTag("DodgeSlider").GetComponent<Slider>();

        // Ensure the SpriteRenderer component is present
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Enemy is missing a SpriteRenderer!");
            return;
        }

        // Ensure the dodge slider is assigned
        if (dodgeSlider == null)
        {
            Debug.LogError("Dodge slider is not assigned!");
            return;
        }

        // Start the attack behavior
        attackCoroutine = StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {


            // Wait for a random interval before the next attack
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);
            isAttacking = true; // Set attacking flag

            // Wind-up phase (change to wind-up color)
            spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            // Attack phase (change to attack color)

            spriteRenderer.color = attackColor;

            // Simulate attack with a brief moment to show the attack color
            float attackDisplayTime = 0.1f; // Duration to show attack color
            yield return new WaitForSeconds(attackDisplayTime);

            // Determine if the player dodged the attack
            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value); // Player's dodge position

            if (playerDodgePosition == enemyAttackPosition)
            {
                Debug.Log("Player hit by attack!");
                EventManager.Instance.TriggerEvent("takeDamageEvent", 10);
            }
            else
            {
                Debug.Log("Player dodged the attack!");
            }

            // Reset the enemy color back to normal
            spriteRenderer.color = normalColor;
            isAttacking = false; // Reset attacking flag
        }
    }

    public bool CanBeHitBy(string attackType)
    {
        switch (attackType)
        {
            case "melee":
                return canBeHitByMelee;
            case "magic":
                return canBeHitByMagic;
            default:
                return false;
        }
    }

    public void TakeDamage()
    {
        if (isAttacking)
        {
            Debug.Log("Enemy cannot be damaged while attacking.");
            return;
        }

        currentHealth -= 1;

        // Flash red when taking damage
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor; // Change to damage color
        yield return new WaitForSeconds(.1f); // Wait for 1 second
        spriteRenderer.color = normalColor; // Revert to normal color
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Clean up the enemy
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        // Stop the attack loop when the enemy is disabled
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
}