using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Keeps track of players health 
/// </summary>
public class PlayerHealth : MonoBehaviour
{

    #region Fields

    // **Health Properties**
    [SerializeField] private int maxHealth = 3; // Maximum health of the player
    private int currentHealth; // Current health of the player, dynamically updated

    // **Visual Feedback**
    [SerializeField] private SpriteRenderer[] spriteRenderers; // Array of sprite renderers (assigned in Inspector)
    private Color originalColor; // Stores the player's original color for flashing effect
    private float flashDuration = 0.2f; // Duration the player flashes red when damaged


    [Header("Screen Flash")]
    [SerializeField] private Image damageFlashImage; // Drag your DamageFlash panel's Image here
    [SerializeField] private float flashFadeSpeed = 5f; // How fast the flash fades away
    private bool isFlashing = false;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Stores the player's original color for later use in visual feedback (flashing effect).
    /// </summary>
    private void Awake()
    {
        if (spriteRenderers.Length > 0)
        {
            originalColor = spriteRenderers[0].color; // Store original color from the first sprite in the array
        }
    }

    /// <summary>
    /// Called when the object is enabled.
    /// Subscribes to the damage and healing events to update the player's health accordingly.
    /// </summary>
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            // Listen for damage and healing events, ensuring the correct methods are called when triggered
            EventManager.Instance.StartListening<int>("takeDamageEvent", TakeDamage);
            EventManager.Instance.StartListening<int>("healDamageEvent", Heal);
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
            EventManager.Instance.StopListening<int>("takeDamageEvent", TakeDamage);
            EventManager.Instance.StopListening<int>("healDamageEvent", Heal);
        }
    }

    /// <summary>
    /// Called when the script starts.
    /// Initializes the player's health and triggers an event to update the UI.
    /// </summary>
    private void Start()
    {
        currentHealth = maxHealth; // Set the player's health to the maximum at the start of the game

        // Notify the system that the player's health has been initialized
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }


    #endregion

    #region PlayerHealthMethods

    /// <summary>
    /// Reduces the player's health when taking damage.
    /// Triggers health update events, applies visual feedback, and checks for death.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    void TakeDamage(int damage)
    {
        if (this == null) return; // Prevent execution if the player object has been destroyed

        // Reduce the player's health and ensure it doesn't go below 0
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Trigger the event to update UI and other systems if EventManager exists
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
        }

        // Apply visual feedback by flashing red if there are sprite renderers
        if (spriteRenderers.Length > 0)
        {
            StartCoroutine(FlashRed());
        }

        // Check if the player has run out of health
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Temporarily flashes the player's sprite red to indicate damage taken.
    /// Returns to the original color after a short delay.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator FlashRed()
    {
        // Flash player sprites
        foreach (var sprite in spriteRenderers)
        {
            sprite.color = Color.red;
        }

        // Flash the screen
        if (damageFlashImage != null)
        {
            isFlashing = true;
            damageFlashImage.color = new Color(1f, 0f, 0f, 0.5f); // Red with 50% opacity
        }

        yield return new WaitForSeconds(flashDuration);

        // Reset player sprites
        foreach (var sprite in spriteRenderers)
        {
            sprite.color = originalColor;
        }

        // Begin fading out the screen flash
        if (damageFlashImage != null)
        {
            StartCoroutine(FadeFlash());
        }
    }

    private IEnumerator FadeFlash()
    {
        while (damageFlashImage.color.a > 0)
        {
            Color currentColor = damageFlashImage.color;
            currentColor.a -= flashFadeSpeed * Time.deltaTime;
            damageFlashImage.color = currentColor;
            yield return null;
        }
        isFlashing = false;
    }

    /// <summary>
    /// Heals the player by the specified amount and ensures health does not exceed the maximum.
    /// Triggers an event to update the UI.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    void Heal(int amount)
    {
        // Increase the player's health and ensure it doesn't exceed the max health
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Trigger the event to update UI and other systems
        EventManager.Instance.TriggerEvent("OnHealthChanged", currentHealth);
    }

    /// <summary>
    /// Handles player death by triggering an event and resetting the game.
    /// Loads the Main Menu upon death.
    /// </summary>
    private void Die()
    {
        // Trigger the "OnPlayerDied" event if the EventManager exists
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent("OnPlayerDied");
        }

        // Reset health before restarting the game
        currentHealth = maxHealth;

        RoundManager.ROUND_NUMBER = 0;

        // Load the main menu scene upon death
        SceneManager.LoadScene("DeathScreen");
    }

    #endregion

}
