using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackingScript : MonoBehaviour
{
    [SerializeField] Slider attackSlider; // Reference to the UI Slider for selecting attack positions
    [SerializeField] Transform leftEnemy; // Reference to the left enemy spawn point
    [SerializeField] Transform centerEnemy; // Reference to the center enemy spawn point
    [SerializeField] Transform rightEnemy; // Reference to the right enemy spawn point

    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right (initial position is center)
    private bool isRoundActive = true; // Track if the current round is active

    private void OnEnable()
    {
        // Subscribe to the "OnStartNewRound" event
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening("OnStartNewRound", StartNewRound);
        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the "OnStartNewRound" event to avoid memory leaks
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("OnStartNewRound", StartNewRound);
        }
    }

    void Start()
    {
        // Initialize the attack slider
        if (attackSlider != null)
        {
            attackSlider.minValue = 0; // Slider's minimum value corresponds to the left position
            attackSlider.maxValue = 2; // Slider's maximum value corresponds to the right position
            attackSlider.wholeNumbers = true; // Slider can only have whole number values
            attackSlider.value = 1; // Set the initial slider position to center
        }
    }

    void Update()
    {
        if (attackSlider != null)
        {
            // Update the selected position based on the slider's value
            selectedPosition = Mathf.RoundToInt(attackSlider.value);

            // Handle slider movement using arrow keys
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveSlider(-1); // Move the slider one position to the left
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveSlider(1); // Move the slider one position to the right
            }
        }

        // Handle melee attack input
        if (Input.GetKeyDown(KeyCode.A)) // Press 'A' to attack with melee
        {
            Attack("melee");
        }

        // Handle magic attack input
        if (Input.GetKeyDown(KeyCode.D)) // Press 'D' to attack with magic
        {
            Attack("magic");
        }

        // Check if all enemies are destroyed and the round is still active
        if (isRoundActive && AllEnemiesDestroyed())
        {
            OnAllEnemiesDestroyed();
        }
    }

    void MoveSlider(int direction)
    {
        if (attackSlider != null)
        {
            // Adjust the slider's value, clamped within the minimum and maximum range
            attackSlider.value = Mathf.Clamp(attackSlider.value + direction, attackSlider.minValue, attackSlider.maxValue);
        }
    }

    public void Attack(string attackType)
    {
        // Determine which enemy to attack based on the selected position
        switch (selectedPosition)
        {
            case 0: // Left position
                AttackEnemy(leftEnemy, attackType);
                break;
            case 1: // Center position
                AttackEnemy(centerEnemy, attackType);
                break;
            case 2: // Right position
                AttackEnemy(rightEnemy, attackType);
                break;
            default:
                Debug.LogError("Invalid slider position");
                break;
        }
    }

    void AttackEnemy(Transform enemy, string attackType)
    {
        if (enemy != null)
        {
            // Loop through all child objects of the enemy (e.g., individual enemy units)
            foreach (Transform child in enemy)
            {
                // Check if the child has an EnemyParent component and can be hit by the specified attack type
                EnemyParent enemyComponent = child.GetComponent<EnemyParent>();
                if (enemyComponent != null && enemyComponent.CanBeHitBy(attackType))
                {
                    enemyComponent.TakeDamage(); // Apply damage to the enemy
                }
            }
        }
        else
        {
            Debug.LogWarning("Enemy transform is not assigned or is null.");
        }
    }

    bool AllEnemiesDestroyed()
    {
        // Check if all enemy spawn points are null or their game objects have been destroyed
        return (leftEnemy == null || leftEnemy.gameObject == null) &&
               (centerEnemy == null || centerEnemy.gameObject == null) &&
               (rightEnemy == null || rightEnemy.gameObject == null);
    }

    void OnAllEnemiesDestroyed()
    {
        isRoundActive = false; // Mark the round as inactive
        EventManager.Instance.TriggerEvent("OnKilledAllEnemies"); // Trigger an event for when all enemies are destroyed
    }

    void StartNewRound()
    {
        isRoundActive = false; // Reset the round state when a new round starts
    }
}
