using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    // Enemy attributes
    [SerializeField] private int maxHealth = 3; // Maximum health for the enemy
    private int currentHealth; // Current health of the enemy

    private Slider dodgeSlider; // Reference to the player's dodge slider
    [SerializeField] private Color normalColor = Color.white; // Default color for the enemy sprite
    [SerializeField] private Color windUpColor = Color.blue; // Color during the wind-up state
    [SerializeField] private Color attackColor = Color.magenta; // Color during the attack state
    [SerializeField] private Color damageColor = Color.red; // Color when taking damage

    // Attack attributes
    private float attackIntervalMin = 1f; // Minimum time between attacks
    private float attackIntervalMax = 2f; // Maximum time between attacks
    private float windUpTime = 0.5f; // Time before the enemy attacks
    [SerializeField] bool canBeHitByMelee = true; // Indicates if the enemy can be hit by melee attacks
    [SerializeField] bool canBeHitByMagic = false; // Indicates if the enemy can be hit by magic attacks
    private float parryCooldown = 2f; // Cooldown time before the player can parry again
    private bool canParry = true; // Tracks if the player can parry

    // Components and state variables
    private SpriteRenderer spriteRenderer; // Reference to the enemy's sprite renderer
    private Coroutine attackCoroutine; // Coroutine reference for the attack loop
    private bool isAttacking = false; // Flag to indicate if the enemy is attacking
    int enemyAttackPosition; // Enemy's attack position

    private void Start()
    {
        // Initialize enemy's health
        currentHealth = maxHealth;

        // Get the enemy's attack position from its parent spawn point
        enemyAttackPosition = GetComponentInParent<SpawnPoint>().SpawnPointNumber;

        // Find the dodge slider in the scene
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

        // Start the attack loop coroutine
        attackCoroutine = StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            // Wait for a random interval before attacking
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);

            isAttacking = true; // Mark the enemy as attacking

            // Wind-up phase
            spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            // Attack phase
            spriteRenderer.color = attackColor;

            bool parrySuccessful = false; // Track if the player successfully parries
            float parryWindow = 0.5f; // Time frame for a parry to occur
            float elapsedTime = 0f;

            // Get the player's dodge position
            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

            // Check for parry within the parry window
            while (elapsedTime < parryWindow)
            {
                if (Input.GetKeyDown(KeyCode.S) && canParry) // Check if parry is available
                {
                    if (playerDodgePosition == enemyAttackPosition)
                    {
                        // Parry successful
                        canParry = false;
                        parrySuccessful = true;
                        Debug.Log("Parry Successful!");
                        spriteRenderer.color = Color.yellow; // Indicate a successful parry
                        yield return new WaitForSeconds(0.2f);
                        spriteRenderer.color = normalColor;

                        // Start the parry cooldown
                        StartCoroutine(ParryCooldown());
                        break;
                    }
                    else
                    {
                        // Parry attempted on the wrong enemy
                        Debug.Log("Parry attempted, but not at the correct enemy!");
                        StartCoroutine(ParryCooldown());
                    }
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!parrySuccessful)
            {
                // Display the attack for a short time
                float attackDisplayTime = 0.5f;
                yield return new WaitForSeconds(attackDisplayTime);

                // Check if the player dodged or was hit
                if (playerDodgePosition == enemyAttackPosition)
                {
                    Debug.Log("Player hit by attack!");
                    EventManager.Instance.TriggerEvent("takeDamageEvent", 10);
                }
                else
                {
                    Debug.Log("Player dodged the attack!");
                }
            }
            else
            {
                Debug.Log("Attack parried successfully, no damage taken!");
            }

            // Reset the enemy's state
            spriteRenderer.color = normalColor;
            isAttacking = false;
        }
    }

    private IEnumerator ParryCooldown()
    {
        // Disable parry and wait for the cooldown duration
        canParry = false;
        yield return new WaitForSeconds(parryCooldown);
        canParry = true; // Re-enable parry
    }

    public bool CanBeHitBy(string attackType)
    {
        // Determine if the enemy can be hit by a specific attack type
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

        // Reduce the enemy's health
        currentHealth -= 1;

        // Flash red to indicate damage
        StartCoroutine(FlashDamage());

        // Check if the enemy should die
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        // Temporarily change the color to indicate damage
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = normalColor;
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Stop the attack coroutine and destroy the enemy
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        // Stop the attack coroutine when the enemy is disabled
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
}
