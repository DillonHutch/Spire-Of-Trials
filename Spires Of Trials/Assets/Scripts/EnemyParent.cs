using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyParent : MonoBehaviour
{
    [SerializeField] Slider dodgeSlider; // Reference to the player's dodge slider
    [SerializeField] List<GameObject> enemies; // List of all enemy GameObjects
    [SerializeField] Color normalColor = Color.white; // Default color
    [SerializeField] Color windUpColor = Color.blue; // Color for wind-up state
    [SerializeField] Color attackColor = Color.magenta; // Color for attack state
    [SerializeField] float attackIntervalMin = 2f; // Minimum time between attacks
    [SerializeField] float attackIntervalMax = 5f; // Maximum time between attacks
    [SerializeField] float windUpTime = 1f; // Time the enemy winds up before attacking

    private GameObject currentAttackingEnemy; // The enemy currently attacking
    private Coroutine attackCoroutine;

    void Start()
    {
        if (dodgeSlider == null)
        {
            Debug.LogError("Dodge slider is not assigned!");
            return;
        }

        if (enemies.Count == 0)
        {
            Debug.LogError("No enemies assigned!");
            return;
        }

        // Start the enemy attack loop
        attackCoroutine = StartCoroutine(EnemyAttackLoop());
    }

    IEnumerator EnemyAttackLoop()
    {
        while (true)
        {
            // Wait for a random interval before the next attack
            float waitTime = Random.Range(attackIntervalMin, attackIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Randomly pick one enemy to attack
            currentAttackingEnemy = enemies[Random.Range(0, enemies.Count)];
            SpriteRenderer spriteRenderer = currentAttackingEnemy.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("Enemy is missing a SpriteRenderer!");
                continue;
            }

            // Wind-up phase (change to wind-up color)
            spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            // Attack phase (change to attack color)
            spriteRenderer.color = attackColor;

            // Wait a brief moment to make the attack color visible
            float attackDisplayTime = 0.5f; // Duration to show attack color
            yield return new WaitForSeconds(attackDisplayTime);

            // Determine the enemy's attack position (0 = Left, 1 = Center, 2 = Right)
            int enemyAttackPosition = enemies.IndexOf(currentAttackingEnemy);

            // Check if the player's dodge position matches the enemy's attack position
            int playerDodgePosition = Mathf.RoundToInt(dodgeSlider.value);

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
        }
    }


    private void OnDestroy()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
}


