using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheildsScript : MonoBehaviour
{
    [SerializeField] private Transform leftShield;
    [SerializeField] private Transform centerShield;
    [SerializeField] private Transform rightShield;



    private bool isRecoiling = false;
    private Coroutine activeRecoilCoroutine;


    // Flash Effect
    protected Coroutine flashCoroutine;
    protected float flashDuration = 0.2f;


    [SerializeField] private float flashTime;
    protected float warningOpacity = 0.50f;


    /// <summary>
    /// Called when the object is enabled.
    /// Subscribes to the damage and healing events to update the player's health accordingly.
    /// </summary>
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            // Listen for damage and healing events, ensuring the correct methods are called when triggered
            EventManager.Instance.StartListening<(
                          Transform leftShield,
                          Transform centerShield,
                          Transform rightShield
                        )>("InitializeShields", data =>
                          InitializeShields(data.leftShield, data.centerShield, data.rightShield));


            EventManager.Instance.StartListening<int>("ShieldRecoil", TriggerShieldRecoil);

            EventManager.Instance.StartListening("ShieldReset", ResetShieldPositions);


            EventManager.Instance.StartListening<SpriteRenderer>("FlashAttack", FlashAttackTrigger);

            
        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    /// <summary>
    /// Called when the object is disabled.
    /// Unsubscribes from the damage and healing events to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            // Listen for damage and healing events, ensuring the correct methods are called when triggered
            EventManager.Instance.StopListening<(
                          Transform leftShield,
                          Transform centerShield,
                          Transform rightShield
                        )>("InitializeShields", data =>
                          InitializeShields(data.leftShield, data.centerShield, data.rightShield));


            EventManager.Instance.StopListening<int>("ShieldRecoil", TriggerShieldRecoil);

            EventManager.Instance.StopListening("ShieldReset", ResetShieldPositions);

            EventManager.Instance.StopListening<SpriteRenderer>("FlashAttack", FlashAttackTrigger);
        }
    }

    /// <summary>
    /// Initializes the shield references.
    /// </summary>
    void InitializeShields(Transform left, Transform center, Transform right)
    {
        leftShield = left;
        centerShield = center;
        rightShield = right;
    }

    /// <summary>
    /// Triggers shield recoil if shield is active and not already recoiling.
    /// </summary>
    void TriggerShieldRecoil(int position)
    {
        Transform shield = GetShieldByPosition(position);
        if (shield != null && shield.gameObject.activeSelf && !isRecoiling)
        {
            isRecoiling = true;
            activeRecoilCoroutine = StartCoroutine(ShieldRecoil(shield));
        }
    }

    /// <summary>
    /// Coroutine for shield recoil effect.
    /// </summary>
    private IEnumerator ShieldRecoil(Transform shield)
    {
        Vector3 originalPosition = shield.position;
        Vector3 recoilPosition = originalPosition + new Vector3(0, -0.2f, 0);

        shield.position = recoilPosition;
        yield return new WaitForSeconds(0.1f);
        shield.position = originalPosition;

        isRecoiling = false;
        activeRecoilCoroutine = null;
    }

    /// <summary>
    /// Gets shield by position.
    /// </summary>
    private Transform GetShieldByPosition(int position)
    {
        return position switch
        {
            0 => leftShield,
            1 => centerShield,
            2 => rightShield,
            _ => null
        };
    }

    /// <summary>
    /// Optionally reset shield positions (e.g. after an attack)
    /// </summary>
    void ResetShieldPositions()
    {
        if (leftShield) leftShield.localPosition = new Vector3(-11f, -9.92f, 0f);
        if (centerShield) centerShield.localPosition = new Vector3(0f, -9.92f, 0f);
        if (rightShield) rightShield.localPosition = new Vector3(11f, -9.92f, 0f);
    }

    void FlashAttackTrigger(SpriteRenderer attackSprite)
    {
        FlashAttackIndicator(attackSprite);
    }


    /// <summary>
    /// Triggers a flashing effect on the attack indicator sprite.
    /// Ensures previous flash coroutines are stopped before starting a new one.
    /// </summary>
    /// <param name="attackSprite">The attack indicator sprite to flash.</param>
    IEnumerator FlashAttackIndicator(SpriteRenderer attackSprite)
    {
        // Ensure the sprite exists before attempting to flash
        if (attackSprite == null) yield break;

        // Make sure the sprite is visible before flashing
        attackSprite.gameObject.SetActive(true);
        attackSprite.enabled = true;

        // Stop any existing flash coroutine to prevent overlapping effects
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Start the flashing effect
        flashCoroutine = StartCoroutine(FlashRoutine(attackSprite));
    }

    /// <summary>
    /// Coroutine that makes the attack indicator sprite flash three times.
    /// </summary>
    /// <param name="attackSprite">The attack indicator sprite to flash.</param>
     IEnumerator FlashRoutine(SpriteRenderer attackSprite)
    {
        // Ensure the sprite exists before attempting to flash
        if (attackSprite == null) yield break;

        // Store the sprite's original color
        Color originalColor = attackSprite.color;

        // Ensure the sprite is enabled before starting the flash effect
        attackSprite.enabled = true;

        // Perform three flashes
        for (int i = 0; i < 3; i++)
        {
            // Change the sprite color to a semi-transparent state
            attackSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, warningOpacity);
            yield return new WaitForSeconds(flashTime);

            // Reset to original color
            attackSprite.color = originalColor;
            yield return new WaitForSeconds(flashTime);
        }

        // Keep the sprite enabled for the next attack indication
        attackSprite.enabled = true;

        // Clear coroutine reference when done
        flashCoroutine = null;
    }

}
