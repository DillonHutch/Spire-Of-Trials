using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

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

    private float meleeCooldown = .2f; // Cooldown duration for melee attacks
    private float rangeCooldown = .2f; // Cooldown duration for range attacks
    private float magicCooldown = .2f; // Cooldown duration for magic attacks

    private float meleeCooldownTimer = 0f; // Current cooldown timer for melee
    private float rangeCooldownTimer = 0f; // Current cooldown timer for range
    private float magicCooldownTimer = 0f; // Current cooldown timer for magic


    private EventInstance currentMusic;
    private List<float> attackTimestamps = new List<float>(); // Stores attack times
    private float attackWindow = 3f; // Time window to track attack speed
    private float highSpeedThreshold = 2; // Attacks per window to trigger fast music


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

        // Get Music Instance
        StartCoroutine(WaitForMusicInstance());
    }

    IEnumerator WaitForMusicInstance()
    {
        yield return new WaitUntil(() => AudioManager.instance != null);
        AudioManager.instance.SetMusic(MusicEnum.Ruins);
        currentMusic = AudioManager.instance.GetCurrentMusicInstance();

        if (currentMusic.isValid())
        {
            currentMusic.setParameterByName("HoMAdaptive", 0);
        }
    }

    void Update()
    {
        HandleCooldownTimers();
        UpdateAttackSpeed();

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
            AudioManager.instance.PlayOneShot( FMODEvents.instance.meleeAttack, this.transform.position);
            StartCooldown(meleeCooldownSlider, ref meleeCooldownTimer, meleeCooldown);
            RegisterAttack();
        }

        // Handle range attack input
        if (Input.GetKeyDown(KeyCode.D) && rangeCooldownTimer <= 0f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rangeAttack, this.transform.position);
            Attack("range");
            StartCooldown(rangeCooldownSlider, ref rangeCooldownTimer, rangeCooldown);
            RegisterAttack();
        }

        // Handle magic attack input
        if (Input.GetKeyDown(KeyCode.S) && magicCooldownTimer <= 0f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.magicAttack, this.transform.position);
            Attack("magic");
            StartCooldown(magicCooldownSlider, ref magicCooldownTimer, magicCooldown);
            RegisterAttack();
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
        switch (attackType)
        {
            case "melee":
                AttackEnemy(GetEnemyAtPosition(selectedPosition), attackType);
                break;
            case "range":
                //int rangeTarget = (selectedPosition == 0) ? 2 : (selectedPosition == 2) ? 0 : -1;
                //if (rangeTarget != -1) AttackEnemy(GetEnemyAtPosition(rangeTarget), attackType);
                AttackEnemy(GetEnemyAtPosition(selectedPosition), attackType);
                break;
            case "magic":
                //if (selectedPosition != 1)
                //{
                //    AttackEnemy(centerEnemy, attackType);
                //}
                AttackEnemy(GetEnemyAtPosition(selectedPosition), attackType);
                break;
            default:
                Debug.LogError("Invalid attack type");
                break;
        }
    }

    private Transform GetEnemyAtPosition(int position)
    {
        return position switch
        {
            0 => leftEnemy,
            1 => centerEnemy,
            2 => rightEnemy,
            _ => null,
        };
    }

    void AttackEnemy(Transform enemy, string attackType)
    {
        if (enemy != null)
        {
            foreach (Transform child in enemy)
            {
                EnemyParent enemyComponent = child.GetComponent<EnemyParent>();
                MiniBoss miniBossComponent = child.GetComponent<MiniBoss>();

                if (enemyComponent != null)
                {
                    enemyComponent.TakeDamage(attackType);
                }
                else if (miniBossComponent != null)
                {
                    //miniBossComponent.TakeDamage(attackType);
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


    void RegisterAttack()
    {
        attackTimestamps.Add(Time.time);
    }

    void UpdateAttackSpeed()
    {
        float currentTime = Time.time;
        attackTimestamps.RemoveAll(t => t < currentTime - attackWindow);
        float attackRate = attackTimestamps.Count / attackWindow;

        if (attackRate >= highSpeedThreshold)
        {
            currentMusic.setParameterByName("HoMAdaptive", 1); // Speed up music
            Debug.LogWarning("music speed up");
        }
        else
        {
            currentMusic.setParameterByName("HoMAdaptive", 0); // Normal speed
            //Debug.LogWarning("music speed down");
        }
    }
}
