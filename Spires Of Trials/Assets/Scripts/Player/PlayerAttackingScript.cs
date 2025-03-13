using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using Unity.VisualScripting;
using TMPro;

public class PlayerAttackingScript : MonoBehaviour
{
    [SerializeField] Slider attackSlider; // Reference to the UI Slider for selecting attack positions
    [SerializeField] GameObject leftSheild;
    [SerializeField] GameObject rightSheild;
    [SerializeField] GameObject middleSheild;
    [SerializeField] private GameObject forwardSprite; // Forward-facing sprite
    [SerializeField] private GameObject sideSprite; // Side-facing sprite (default left)
    [SerializeField] Transform leftEnemy; // Reference to the left enemy spawn point
    [SerializeField] Transform centerEnemy; // Reference to the center enemy spawn point
    [SerializeField] Transform rightEnemy; // Reference to the right enemy spawn point

    private int selectedPosition = 1; // 0 = Left, 1 = Center, 2 = Right (initial position is center)
    private bool isRoundActive = true; // Track if the current round is active

    private float meleeCooldownTimer = 0f; // Current cooldown timer for melee
    private float rangeCooldownTimer = 0f; // Current cooldown timer for range
    private float magicCooldownTimer = 0f; // Current cooldown timer for magic
    private float heavyCooldownTimer = 0f; // Current cooldown timer for heavy

    private EventInstance currentMusic;
    private List<float> attackTimestamps = new List<float>(); // Stores attack times
    private float attackWindow = 3f; // Time window to track attack speed
    private float highSpeedThreshold = 2; // Attacks per window to trigger fast music

    [SerializeField] SpriteRenderer sideSpriteRenderer;

    private int comboCount = 0; // Tracks consecutive successful attacks
    [SerializeField] private TextMeshProUGUI comboText; // UI display for combo (optional)


    private bool isShaking = false; // Prevent multiple shakes from running at the same time

    const int ComboGot = 24;
    const int GreaterCombo = 64;
    const int FinalCombo = 120;


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
        //InitializeCooldownSlider(meleeCooldownSlider, meleeCooldown);
        //InitializeCooldownSlider(rangeCooldownSlider, rangeCooldown);
        //InitializeCooldownSlider(magicCooldownSlider, magicCooldown);
        //InitializeCooldownSlider(heavyCooldownSlider, heavyCooldown);


        // Get Music Instance
        StartCoroutine(WaitForMusicInstance());
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
                    miniBossComponent.TakeDamage(attackType);
                    
                }
            }
        }

        
    }

    public void UpdateCombo(bool hit)
    {
        if (hit)
        {
            comboCount++; // Increase combo if the attack lands
        }
        else
        {
            comboCount = 0; // Reset combo on a miss
        }

        // Ensure the UI is updated if assigned
        if (comboText != null)
        {
            comboText.text = "Combo: " + comboCount;

            // Apply visual changes based on combo count
            if (comboCount >= ComboGot) // First threshold (5+)
            {
                comboText.color = Color.red;
                comboText.fontSize = 55; // Slightly bigger
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(5f, 0.5f)); // Mild shake
                }
            }
            if (comboCount >= GreaterCombo) // Second threshold (10+)
            {
                comboText.color = Color.yellow;
                comboText.fontSize = 60; // Bigger
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(7f, 0.8f)); // Stronger shake
                }
            }
            if (comboCount >= FinalCombo) // Final threshold (15+)
            {
                comboText.color = Color.cyan; // Final effect
                comboText.fontSize = 65; // Even bigger
                if (!isShaking)
                {
                    StartCoroutine(ShakeText(12f, 1f)); // Maximum shake
                }
            }
            else if (comboCount < ComboGot) // Reset if below first threshold
            {
                comboText.color = Color.white;
                comboText.fontSize = 40; // Default size
            }

            // Audio trigger for 5 combo milestone
            if (comboCount == ComboGot)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.combo, transform.position);
            }
            else if(comboCount == GreaterCombo)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.unstopable, transform.position);
            }
            else if(comboCount == FinalCombo)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.legendary, transform.position);
            }
        }
    }





    IEnumerator ShakeText(float magnitude, float duration)
    {
        isShaking = true;
        Vector3 originalPosition = comboText.transform.localPosition;
        float elapsed = 0f;

        while (comboCount >= ComboGot) // Keep shaking while combo is active
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

        // Reset position after shaking stops
        comboText.transform.localPosition = originalPosition;
        isShaking = false;
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

        // Dodge movement using A and D keys
        if (Input.GetKey(KeyCode.A))
        {
            attackSlider.value = 0; // Move to left (dodge left)
            ShowSideSprite(facingLeft: true);
            leftSheild.SetActive(true);
            rightSheild.SetActive(false);
            middleSheild.SetActive(false);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            attackSlider.value = 2; // Move to right (dodge right)
            ShowSideSprite(facingLeft: false);
            leftSheild.SetActive(false);
            rightSheild.SetActive(true);
            middleSheild.SetActive(false);
        }
        else
        {
            attackSlider.value = 1; // Return to center
            ShowForwardSprite();
            leftSheild.SetActive(false);
            rightSheild.SetActive(false);
            middleSheild.SetActive(true);
        }

        selectedPosition = Mathf.RoundToInt(attackSlider.value);

        // Attacks using arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow) && heavyCooldownTimer <= 0f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.heavyAttack, this.transform.position);
            Attack("heavy");
            //StartCooldown(heavyCooldownSlider, ref heavyCooldownTimer, heavyCooldown);
            RegisterAttack();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && meleeCooldownTimer <= 0f)
        {
            Attack("melee");
            AudioManager.instance.PlayOneShot(FMODEvents.instance.meleeAttack, this.transform.position);
            //StartCooldown(meleeCooldownSlider, ref meleeCooldownTimer, meleeCooldown);
            RegisterAttack();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && rangeCooldownTimer <= 0f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.rangeAttack, this.transform.position);
            Attack("range");
            //StartCooldown(rangeCooldownSlider, ref rangeCooldownTimer, rangeCooldown);
            RegisterAttack();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && magicCooldownTimer <= 0f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.magicAttack, this.transform.position);
            Attack("magic");
            //StartCooldown(magicCooldownSlider, ref magicCooldownTimer, magicCooldown);
            RegisterAttack();
        }

        if (isRoundActive && AllEnemiesDestroyed())
        {
            OnAllEnemiesDestroyed();
        }
    }


    void SetAlpha(SpriteRenderer sr, float alpha)
    {
   
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }       
    }



    void ShowForwardSprite()
    {
        forwardSprite.SetActive(true);
        sideSprite.SetActive(false);
    }

    void ShowSideSprite(bool facingLeft)
    {
        forwardSprite.SetActive(false);
        sideSprite.SetActive(true);

        if (facingLeft)
        {
            sideSpriteRenderer.flipX = false;
        }
        else
        {
            sideSpriteRenderer.flipX = true;
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
            case "range":
            case "magic":
            case "heavy": // Add heavy attack type
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

  


    void HandleCooldownTimers()
    {
        //UpdateCooldownSlider(meleeCooldownSlider, ref meleeCooldownTimer, meleeCooldown);
        //UpdateCooldownSlider(rangeCooldownSlider, ref rangeCooldownTimer, rangeCooldown);
        //UpdateCooldownSlider(magicCooldownSlider, ref magicCooldownTimer, magicCooldown);
        //UpdateCooldownSlider(heavyCooldownSlider, ref heavyCooldownTimer, heavyCooldown);

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
