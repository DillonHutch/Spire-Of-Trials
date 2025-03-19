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
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening("OnHealthChanged", UpdateHealthBar);
        }
        else
        {
            Debug.LogError("EventManager instance is null. Ensure it is present in the scene.");
        }
    }

    /// <summary>
    /// Called when the object is disabled.
    /// Unsubscribes from the "OnHealthChanged" event to prevent memory leaks and unintended behavior.
    /// </summary>
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("OnHealthChanged", UpdateHealthBar);
        }
    }

    #endregion

    #region UpdateHealthBar
    /// <summary>
    /// Updates the health bar UI when the player's health changes.
    /// This method is triggered by the "OnHealthChanged" event.
    /// </summary>
    /// <param name="health">The current health value, passed as an object.</param>
    private void UpdateHealthBar(object health)
    {
        // Convert the received health object to an integer
        int currentHealth = (int)health;

        // Update the health slider's value to reflect the new health
        healthSlider.value = currentHealth;

        // Calculate the health percentage (normalized between 0 and 1)
        float healthPercentage = healthSlider.normalizedValue;

        // Adjust the fill image color based on the current health percentage using a gradient
        fillImage.color = healthGradient.Evaluate(healthPercentage);
    }


    #endregion
}
