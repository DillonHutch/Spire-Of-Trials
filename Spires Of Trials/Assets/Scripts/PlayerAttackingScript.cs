using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackingScript : MonoBehaviour
{
    [SerializeField] Slider attackSlider; // Reference to the UI Slider
    [SerializeField] Transform leftEnemy;
    [SerializeField] Transform centerEnemy;
    [SerializeField] Transform rightEnemy;
   // [SerializeField] float attackRange = 2f; // Range within which an attack can hit

    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right
    private bool isRoundActive = true; // Track if the round is active



    private void OnEnable()
    {
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
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("OnStartNewRound", StartNewRound);
        }
    }

    void Start()
    {
        if (attackSlider != null)
        {
            attackSlider.minValue = 0;
            attackSlider.maxValue = 2;
            attackSlider.wholeNumbers = true;
            attackSlider.value = 1; // Start at the center
        }
    }

    void Update()
    {
        if (attackSlider != null)
        {
            selectedPosition = Mathf.RoundToInt(attackSlider.value);
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Press Space to attack
        {
            Attack();
        }

        // Check if all enemies are destroyed and if the round is active
        if (isRoundActive && AllEnemiesDestroyed())
        {
            OnAllEnemiesDestroyed();
        }
    }

    public void Attack()
    {
        switch (selectedPosition)
        {
            case 0:
                AttackEnemy(leftEnemy);
                break;
            case 1:
                AttackEnemy(centerEnemy);
                break;
            case 2:
                AttackEnemy(rightEnemy);
                break;
            default:
                Debug.LogError("Invalid slider position");
                break;
        }
    }

    void AttackEnemy(Transform enemy)
    {
        if (enemy != null)
        {
            foreach (Transform child in enemy)
            {
                Destroy(child.gameObject); // Destroy each child GameObject
            }
        }
        else
        {
            Debug.LogWarning("Enemy transform is not assigned or is null.");
        }
    }

    bool AllEnemiesDestroyed()
    {
        return (leftEnemy == null || leftEnemy.gameObject == null) &&
               (centerEnemy == null || centerEnemy.gameObject == null) &&
               (rightEnemy == null || rightEnemy.gameObject == null);
    }

    void OnAllEnemiesDestroyed()
    {
        isRoundActive = false; // Prevent the event from being repeatedly triggered
        EventManager.Instance.TriggerEvent("OnKilledAllEnemies");
    }

    void StartNewRound()
    {
        isRoundActive = false; // Reset round state when starting a new round
    }
}

