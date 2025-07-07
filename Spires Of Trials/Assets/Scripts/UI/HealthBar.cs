using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// health bar class
/// </summary>
public class HealthBar : MonoBehaviour
{

    #region Fields

    [SerializeField] Slider healthSlider;
    [SerializeField] Image fillImage; // Reference to the fill image of the slider
    [SerializeField] Gradient healthGradient; // Gradient for color transition (green to red)


    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Ensures the health bar's fill image is assigned properly.
    /// </summary>
    private void Awake()
    {
        if (fillImage == null)
        {
            // Find the Fill image inside the health slider if it is not manually assigned in the Inspector.
            fillImage = healthSlider.fillRect.GetComponentInChildren<Image>();
        }
    }

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Subscribes to the "OnHealthChanged" event to update the health bar when the player's health changes.
    /// Ensures the EventManager instance exists before subscribing.
    /// </summary>
    private void OnEnable()
    {
        // subscribe
        EventManager.Instance.StartListening<int>(
            "OnHealthChanged",
            UpdateHealthBar
        );

        // pull in the current values
        PlayerHealth playerHealth = PlayerHealth.Instance;
        healthSlider.maxValue = playerHealth.MaxHealth;
        UpdateHealthBar(playerHealth.CurrentHealth);
    }

    /// <summary>
    /// Called when the object is disabled.
    /// Unsubscribes from the "OnHealthChanged" event to prevent memory leaks and unintended behavior.
    /// </summary>
    private void OnDisable()
    {
        EventManager.Instance.StopListening<int>(
            "OnHealthChanged",
            UpdateHealthBar
        );
    }




    #endregion

    #region UpdateHealthBar
    /// <summary>
    /// Updates the health bar UI when the player's health changes.
    /// This method is triggered by the "OnHealthChanged" event.
    /// </summary>
    /// <param name="health">The current health value, passed as an object.</param>
    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        float normalized = healthSlider.normalizedValue;
        fillImage.color = healthGradient.Evaluate(normalized);
    }


    #endregion
}
