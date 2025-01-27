using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAttack : MonoBehaviour
{
    private Slider dodgeSlider; // Reference to the player's dodge slider
    [SerializeField] private Color normalColor = Color.white; // Default color
    [SerializeField] private Color windUpColor = Color.blue; // Color for wind-up state
    [SerializeField] private Color attackColor = Color.magenta; // Color for attack state
    [SerializeField] private float attackIntervalMin = 2f; // Minimum time between attacks
    [SerializeField] private float attackIntervalMax = 5f; // Maximum time between attacks
    [SerializeField] private float windUpTime = 1f; // Time the enemy winds up before attacking

    private SpriteRenderer spriteRenderer;
    private Coroutine attackCoroutine;


    int enemyAttackPosition;

    private void Start()
    {


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

            // Wind-up phase (change to wind-up color)
            spriteRenderer.color = windUpColor;
            yield return new WaitForSeconds(windUpTime);

            // Attack phase (change to attack color)
            spriteRenderer.color = attackColor;

            // Simulate attack with a brief moment to show the attack color
            float attackDisplayTime = 0.5f; // Duration to show attack color
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
        }
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
