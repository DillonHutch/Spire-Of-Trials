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
    [SerializeField] float attackRange = 2f; // Range within which an attack can hit

    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right

    void Start()
    {
        // Ensure the slider is set up properly
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
            
          enemy.gameObject.SetActive(false);

        }
        else
        {
            Debug.LogWarning("Enemy transform is not assigned.");
        }
    }
}
