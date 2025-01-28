using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackingScript : MonoBehaviour
{
    [SerializeField] Slider attackSlider; // Reference to the UI Slider for selecting attack positions
    [SerializeField] Slider meleeCooldownSlider; // Slider for melee cooldown
    [SerializeField] Slider rangeCooldownSlider; // Slider for range cooldown
    [SerializeField] Slider magicCooldownSlider; // Slider for magic cooldown

    [SerializeField] Transform leftEnemy; // Reference to the left enemy spawn point
    [SerializeField] Transform centerEnemy; // Reference to the center enemy spawn point
    [SerializeField] Transform rightEnemy; // Reference to the right enemy spawn point

    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right (initial position is center)
    private bool isRoundActive = true; // Track if the current round is active

    private float meleeCooldown = .4f; // Cooldown duration for melee attacks
    private float rangeCooldown = .4f; // Cooldown duration for range attacks
    private float magicCooldown = .4f; // Cooldown duration for magic attacks

    private float meleeCooldownTimer = 0f; // Current cooldown timer for melee
    private float rangeCooldownTimer = 0f; // Current cooldown timer for range
    private float magicCooldownTimer = 0f; // Current cooldown timer for magic

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
        // Initialize the attack slider
        if (attackSlider != null)
        {
            attackSlider.minValue = 0;
            attackSlider.maxValue = 2;
            attackSlider.wholeNumbers = true;
            attackSlider.value = 1;
        }

        // Initialize cooldown sliders
        InitializeCooldownSlider(meleeCooldownSlider, meleeCooldown);
        InitializeCooldownSlider(rangeCooldownSlider, rangeCooldown);
        InitializeCooldownSlider(magicCooldownSlider, magicCooldown);
    }

    void Update()
    {
        HandleCooldownTimers();

        if (attackSlider != null)
        {
            selectedPosition = Mathf.RoundToInt(attackSlider.value);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveSlider(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveSlider(1);
            }
        }

        // Handle melee attack input
        if (Input.GetKeyDown(KeyCode.A) && meleeCooldownTimer <= 0f)
        {
            Attack("melee");
            StartCooldown(meleeCooldownSlider, ref meleeCooldownTimer, meleeCooldown);
        }

        // Handle range attack input
        if (Input.GetKeyDown(KeyCode.S) && rangeCooldownTimer <= 0f)
        {
            Attack("range");
            StartCooldown(rangeCooldownSlider, ref rangeCooldownTimer, rangeCooldown);
        }

        // Handle magic attack input
        if (Input.GetKeyDown(KeyCode.D) && magicCooldownTimer <= 0f)
        {
            Attack("magic");
            StartCooldown(magicCooldownSlider, ref magicCooldownTimer, magicCooldown);
        }

        if (isRoundActive && AllEnemiesDestroyed())
        {
            OnAllEnemiesDestroyed();
        }
    }

    void MoveSlider(int direction)
    {
        if (attackSlider != null)
        {
            attackSlider.value = Mathf.Clamp(attackSlider.value + direction, attackSlider.minValue, attackSlider.maxValue);
        }
    }

    public void Attack(string attackType)
    {
        switch (selectedPosition)
        {
            case 0:
                AttackEnemy(leftEnemy, attackType);
                break;
            case 1:
                AttackEnemy(centerEnemy, attackType);
                break;
            case 2:
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
            foreach (Transform child in enemy)
            {
                EnemyParent enemyComponent = child.GetComponent<EnemyParent>();
                if (enemyComponent != null && enemyComponent.CanBeHitBy(attackType))
                {
                    enemyComponent.TakeDamage();
                }
            }
        }
        else
        {
            Debug.LogWarning("Enemy transform is not assigned or is null.");
        }
    }

    void HandleCooldownTimers()
    {
        UpdateCooldownSlider(meleeCooldownSlider, ref meleeCooldownTimer, meleeCooldown);
        UpdateCooldownSlider(rangeCooldownSlider, ref rangeCooldownTimer, rangeCooldown);
        UpdateCooldownSlider(magicCooldownSlider, ref magicCooldownTimer, magicCooldown);
    }

    void InitializeCooldownSlider(Slider slider, float maxCooldown)
    {
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = maxCooldown;
            slider.value = 100; // Start empty
        }
    }

    void StartCooldown(Slider slider, ref float timer, float duration)
    {
        if (slider != null)
        {
            timer = duration;
            slider.value = 0; // Reset to empty
        }
    }

    void UpdateCooldownSlider(Slider slider, ref float timer, float maxCooldown)
    {
        if (slider != null && timer > 0f)
        {
            timer -= Time.deltaTime;
            slider.value = Mathf.Clamp(maxCooldown - timer, 0f, slider.maxValue); // Fill up as cooldown progresses
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
        isRoundActive = false;
        EventManager.Instance.TriggerEvent("OnKilledAllEnemies");
    }

    void StartNewRound()
    {
        isRoundActive = true;
    }
}
