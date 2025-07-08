using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using Unity.VisualScripting;
using TMPro;

/// <summary>
/// for when the player attacks
/// </summary>
public class PlayerAttackingScript : MonoBehaviour
{

    #region Fields
    // **UI Elements**
    [Header("UI Elements")]
    [SerializeField] private Slider attackSlider; // UI Slider for selecting attack positions
    [SerializeField] private TextMeshProUGUI comboText; // UI display for combo (optional)

    // **Shield References**
    [Header("Shield References")]
    [SerializeField] private GameObject leftShield;   // Left shield object
    [SerializeField] private GameObject rightShield;  // Right shield object
    [SerializeField] private GameObject middleShield; // Middle shield object

    // **Player Sprites**
    [Header("Player Sprites")]
    [SerializeField] private GameObject forwardSprite; // Forward-facing sprite
    [SerializeField] private GameObject sideSprite;    // Side-facing sprite (default left)
    [SerializeField] private SpriteRenderer sideSpriteRenderer; // Renderer for the side sprite

    // **Enemy Spawn Points**
    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform leftEnemy;   // Reference to the left enemy spawn point
    [SerializeField] private Transform centerEnemy; // Reference to the center enemy spawn point
    [SerializeField] private Transform rightEnemy;  // Reference to the right enemy spawn point

    // **Player Positioning**
    [Header("Player Positioning")]
    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right (Initial position is center)
    private bool isRoundActive = true; // Tracks if the current round is active

    // **Attack Cooldown Timers**
    [Header("Attack Cooldowns")]
    private float meleeCooldownTimer = 0f; // Current cooldown timer for melee attacks
    private float rangeCooldownTimer = 0f; // Current cooldown timer for ranged attacks
    private float magicCooldownTimer = 0f; // Current cooldown timer for magic attacks
    private float heavyCooldownTimer = 0f; // Current cooldown timer for heavy attacks

    // **Attack Speed Tracking**
    [Header("Attack Speed Tracking")]
    private List<float> attackTimestamps = new List<float>(); // Stores attack timestamps
    private float attackWindow = 3f; // Time window to track attack speed
    private float highSpeedThreshold = 2; // Number of attacks per window required to trigger fast music

    // **Combo Tracking**
    [Header("Combo Tracking")]
    private int comboCount = 0; // Tracks consecutive successful attacks
    const int ComboGot = 24;     // First milestone combo
    const int GreaterCombo = 50; // Second milestone combo
    const int FinalCombo = 100;  // Final milestone combo

    // **Visual & Audio Effects**
    [Header("Visual & Audio Effects")]
    private bool isShaking = false; // Prevents multiple screen shakes from running simultaneously
    private EventInstance currentMusic; // Reference to the current background music event

    private bool isCrouching = false;

    #endregion


    public int GetCurrentAttackPosition()
    {
        return selectedPosition;
    }


    #region UnityMethods

    /// <summary>
    /// Called when the script is enabled.
    /// Subscribes to the "OnStartNewRound" event to reset the round when triggered.
    /// </summary>
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening("OnStartNewRound", StartNewRound);            
            EventManager.Instance.StartListening<bool>("UpdateCombo", UpdateCombo);

        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    /// <summary>
    /// Called when the script is disabled.
    /// Unsubscribes from the "OnStartNewRound" event to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("OnStartNewRound", StartNewRound);
            EventManager.Instance.StopListening<bool>("UpdateCombo", UpdateCombo);
        }
    }

    /// <summary>
    /// Called when the script starts.
    /// Initializes the attack slider and begins waiting for the music instance.
    /// </summary>
    void Start()
    {
        // Initialize the attack slider if assigned in the Inspector
        if (attackSlider != null)
        {
            attackSlider.minValue = 0; // Left position
            attackSlider.maxValue = 2; // Right position
            attackSlider.wholeNumbers = true; // Ensure only integer values (0, 1, 2)
            attackSlider.value = 1; // Default to center position
        }

        // Start coroutine to get the music instance
        StartCoroutine(WaitForMusicInstance());
    }

    /// <summary>
    /// Called every frame.
    /// Handles player movement (dodging), attack input, and attack speed tracking.
    /// </summary>
    void Update()
    {
        if (!TimingController.Instance.FightActive)
            return;

        // 1) Always handle dodge movement & shield positioning
        HandleDodgeMovement();

        // 2) Check for crouch (Shift held)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                
            }
            // while crouching, do not process any attack inputs:
            return;
        }
        else if (isCrouching)
        {
            isCrouching = false;
            
        }

        // 3) Only skip attacks in skill‐phase
        if (!TimingController.Instance.SkillPhase)
        {
            HandleAttackInputs();
        }

        // 4) Round‐complete check
        if (isRoundActive && AllEnemiesDestroyed())
        {
            OnAllEnemiesDestroyed();
        }
    }



    private void HandleDodgeMovement()
{
    if (Input.GetKey(KeyCode.A))
    {
        attackSlider.value = 0;
        ShowSideSprite(facingLeft: true);
        leftShield.SetActive(true);
        middleShield.SetActive(false);
        rightShield.SetActive(false);
    }
    else if (Input.GetKey(KeyCode.D))
    {
        attackSlider.value = 2;
        ShowSideSprite(facingLeft: false);
        rightShield.SetActive(true);
        middleShield.SetActive(false);
        leftShield.SetActive(false);
    }
    else
    {
        attackSlider.value = 1;
        ShowForwardSprite();
        middleShield.SetActive(true);
        leftShield.SetActive(false);
        rightShield.SetActive(false);
    }
    selectedPosition = Mathf.RoundToInt(attackSlider.value);
}

private void HandleAttackInputs()
{
    if (Input.GetKeyDown(KeyCode.UpArrow) && heavyCooldownTimer <= 0f)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.heavyAttack, transform.position);
        Attack("heavy");
        RegisterAttack();
    }
    if (Input.GetKeyDown(KeyCode.LeftArrow) && meleeCooldownTimer <= 0f)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.meleeAttack, transform.position);
        Attack("melee");
        RegisterAttack();
    }
    if (Input.GetKeyDown(KeyCode.RightArrow) && rangeCooldownTimer <= 0f)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.rangeAttack, transform.position);
        Attack("range");
        RegisterAttack();
    }
    if (Input.GetKeyDown(KeyCode.DownArrow) && magicCooldownTimer <= 0f)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.magicAttack, transform.position);
        Attack("magic");
        RegisterAttack();
    }
}



    #endregion

    #region AttackSpeedMethods

    /// <summary>
    /// Waits for the AudioManager instance to become available before setting the background music.
    /// Ensures the correct music theme is played and initializes music parameters.
    /// </summary>
    IEnumerator WaitForMusicInstance()
    {
        // Wait until the AudioManager instance is ready
        yield return new WaitUntil(() => AudioManager.instance != null);

        // Set the background music to "Ruins" theme
        //AudioManager.instance.SetMusic(MusicEnum.Ruins);

        // Get the current music instance
        currentMusic = AudioManager.instance.GetCurrentMusicInstance();

        // Ensure the music instance is valid before modifying parameters
        if (currentMusic.isValid())
        {
            // Set the adaptive music parameter to its default state
            currentMusic.setParameterByName("HoMAdaptive", 0);
        }
    }

    /// <summary>
    /// Registers an attack by storing its timestamp.
    /// Used to track attack speed over time.
    /// </summary>
    void RegisterAttack()
    {
        attackTimestamps.Add(Time.time);
    }

    /// <summary>
    /// Updates the attack speed tracking system.
    /// Adjusts music tempo based on attack rate within a set time window.
    /// </summary>
    void UpdateAttackSpeed()
    {
        float currentTime = Time.time;

        // Remove outdated attack timestamps that fall outside the tracking window
        attackTimestamps.RemoveAll(t => t < currentTime - attackWindow);

        // Calculate the attack rate (attacks per second)
        float attackRate = attackTimestamps.Count / attackWindow;

        // Adjust the music tempo based on the attack speed
        if (attackRate >= highSpeedThreshold)
        {
            currentMusic.setParameterByName("HoMAdaptive", 1); // Speed up music
            //Debug.LogWarning("Music speed up");
        }
        else
        {
            currentMusic.setParameterByName("HoMAdaptive", 0); // Normal speed
                                                               // Debug.LogWarning("Music speed down");
        }
    }

    #endregion

    #region PlayerAttackMethods

    /// <summary>
    /// Attacks the specified enemy by dealing damage based on the attack type.
    /// Supports both normal enemies and MiniBosses.
    /// </summary>
    /// <param name="enemy">The enemy Transform to attack.</param>
    /// <param name="attackType">The type of attack being used.</param>
    void AttackEnemy(Transform enemy, string attackType)
    {
        if (enemy != null)
        {
            foreach (Transform child in enemy)
            {
                // Try to get the enemy components
                EnemyParent enemyComponent = child.GetComponent<EnemyParent>();
                MiniBoss miniBossComponent = child.GetComponent<MiniBoss>();

                // Deal damage if the enemy component exists
                if (enemyComponent != null)
                {
                    enemyComponent.TakeDamage(attackType);
                }
                else if (miniBossComponent != null)
                {
                    miniBossComponent.TakeDamage(attackType);
                }
            }
        }
    }

    /// <summary>
    /// Handles the player's attack based on the given attack type.
    /// Determines the enemy at the player's position and applies the correct attack.
    /// </summary>
    /// <param name="attackType">The type of attack (melee, range, magic, heavy).</param>
     void Attack(string attackType)
    {
        switch (attackType)
        {
            case "melee":
            case "range":
            case "magic":
            case "heavy": // Supports heavy attack type
                AttackEnemy(GetEnemyAtPosition(selectedPosition), attackType);
                break;
            default:
                Debug.LogError("Invalid attack type");
                break;
        }
    }

    /// <summary>
    /// Retrieves the enemy Transform at the specified position.
    /// </summary>
    /// <param name="position">The player's current attack position (0 = left, 1 = center, 2 = right).</param>
    /// <returns>The Transform of the enemy at the given position, or null if no enemy exists there.</returns>
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

    /// <summary>
    /// Checks if all enemies have been destroyed.
    /// Returns true if no active enemies remain in any position.
    /// </summary>
    /// <returns>True if all enemies are destroyed, otherwise false.</returns>
    bool AllEnemiesDestroyed()
    {
        return (leftEnemy == null || leftEnemy.gameObject == null) &&
               (centerEnemy == null || centerEnemy.gameObject == null) &&
               (rightEnemy == null || rightEnemy.gameObject == null);
    }

    /// <summary>
    /// Handles the event when all enemies have been defeated.
    /// Ends the round and triggers the "OnKilledAllEnemies" event.
    /// </summary>
    void OnAllEnemiesDestroyed()
    {
        isRoundActive = false;
        EventManager.Instance.TriggerEvent("OnKilledAllEnemies");
    }

    #endregion

    #region ComboMethods

    /// <summary>
    /// Updates the combo count based on whether the player successfully landed an attack.
    /// Modifies UI elements and triggers special effects when reaching combo milestones.
    /// </summary>
    /// <param name="hit">True if the attack lands, false if it misses.</param>
     void UpdateCombo(bool hit)
    {
        if (hit)
        {
            comboCount++; // Increase combo count if the attack lands successfully
        }
        else
        {
            comboCount = 0; // Reset combo count on a miss
        }

        // Ensure the UI updates only if the comboText object is assigned
        if (comboText != null)
        {
            // Display the updated combo count
            comboText.text = "Combo: " + comboCount;

            // Apply visual effects based on the current combo count
            if (comboCount >= ComboGot) // First milestone
            {
                comboText.color = Color.red;
                comboText.fontSize = 55; // Slightly larger font
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(5f, 0.5f)); // Mild shake effect
                }
            }
            if (comboCount >= GreaterCombo) // Second milestone
            {
                comboText.color = Color.yellow;
                comboText.fontSize = 60; // Larger font
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(7f, 0.8f)); // Stronger shake effect
                }
            }
            if (comboCount >= FinalCombo) // Final milestone
            {
                comboText.color = Color.cyan; // Ultimate visual effect
                comboText.fontSize = 65; // Even larger font
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(12f, 1f)); // Maximum shake effect
                }
            }
            else if (comboCount < ComboGot) // Reset to default if below first milestone
            {
                comboText.color = Color.white;
                comboText.fontSize = 40; // Default font size
            }

            // Play unique sound effects when reaching specific combo milestones
            if (comboCount == ComboGot)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.combo, transform.position);
            }
            else if (comboCount == GreaterCombo)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.unstopable, transform.position);
            }
            else if (comboCount == FinalCombo)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.legendary, transform.position);
            }
        }
    }

    /// <summary>
    /// Coroutine that applies a shaking effect to the combo text when the combo count is high.
    /// The intensity of the shake increases with the combo tier.
    /// </summary>
    /// <param name="magnitude">The intensity of the shake.</param>
    /// <param name="duration">The duration of the shake.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    IEnumerator ShakeText(float magnitude, float duration)
    {
        isShaking = true;
        Vector3 originalPosition = comboText.transform.localPosition;
        float elapsed = 0f;

        // Keep shaking as long as the combo count remains above the first milestone
        while (comboCount >= ComboGot)
        {
            elapsed += Time.deltaTime;

            // Increase shake intensity for each combo tier
            float strength = (comboCount >= FinalCombo) ? magnitude * 2f :
                             (comboCount >= GreaterCombo) ? magnitude * 1.5f :
                             magnitude;

            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            comboText.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            yield return null;
        }

        // Reset text position once the shaking stops
        comboText.transform.localPosition = originalPosition;
        isShaking = false;
    }

    #endregion

    #region PlayerPositionMethods

    /// <summary>
    /// Moves the attack slider in the given direction while ensuring it stays within the allowed range.
    /// </summary>
    /// <param name="direction">The direction to move the slider (-1 for left, +1 for right).</param>
    void MoveSlider(int direction)
    {
        if (attackSlider != null)
        {
            // Adjust the slider value and clamp it within the defined min and max values
            attackSlider.value = Mathf.Clamp(attackSlider.value + direction, attackSlider.minValue, attackSlider.maxValue);
        }
    }

    /// <summary>
    /// Displays the forward-facing sprite while hiding the side-facing sprite.
    /// Used when the player is in the center position.
    /// </summary>
    void ShowForwardSprite()
    {
        forwardSprite.SetActive(true);  // Enable forward-facing sprite
        sideSprite.SetActive(false);    // Disable side-facing sprite
    }

    /// <summary>
    /// Displays the side-facing sprite while hiding the forward-facing sprite.
    /// Adjusts the sprite orientation based on the direction the player is facing.
    /// </summary>
    /// <param name="facingLeft">True if the player is facing left, false if facing right.</param>
    void ShowSideSprite(bool facingLeft)
    {
        forwardSprite.SetActive(false); // Disable forward-facing sprite
        sideSprite.SetActive(true);     // Enable side-facing sprite

        // Adjust the sprite flipping based on the direction the player is facing
        sideSpriteRenderer.flipX = !facingLeft;
    }

    #endregion

    #region RoundSupport

    /// <summary>
    /// Starts a new round by setting the round state to active.
    /// This allows the player to engage in combat again.
    /// </summary>
    void StartNewRound()
    {
        isRoundActive = true;
    }

    #endregion

}
