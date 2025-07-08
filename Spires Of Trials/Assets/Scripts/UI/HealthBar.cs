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

    private IEnumerator Start()
    {
        // wait until PlayerHealth has initialized its Instance
        yield return new WaitUntil(() => PlayerHealth.Instance != null);

        // sanity-check your references
        if (healthSlider == null)
        {
            Debug.LogError("HealthBar: healthSlider is not assigned!");
            yield break;
        }

        if (EventManager.Instance == null)
        {
            Debug.LogError("HealthBar: EventManager.Instance is null!");
            yield break;
        }

        // now safe to subscribe and pull initial values
        EventManager.Instance.StartListening<int>("OnHealthChanged", UpdateHealthBar);
        healthSlider.maxValue = PlayerHealth.Instance.MaxHealth;
        UpdateHealthBar(PlayerHealth.Instance.CurrentHealth);
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
        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);
    }


    #endregion
}
