using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider healthSlider;
    [SerializeField] Image fillImage; // Reference to the fill image of the slider
    [SerializeField] Gradient healthGradient; // Gradient for color transition (green to red)

    private void Awake()
    {
        if (fillImage == null)
        {
            // Find the Fill image inside the slider if not manually assigned
            fillImage = healthSlider.fillRect.GetComponentInChildren<Image>();
        }
    }

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

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening("OnHealthChanged", UpdateHealthBar);
        }
    }

    private void UpdateHealthBar(object health)
    {
        int currentHealth = (int)health;

        // Update the slider value
        healthSlider.value = currentHealth;

        // Update the fill image color based on the slider's normalized value
        float healthPercentage = healthSlider.normalizedValue; // Value between 0 and 1
        fillImage.color = healthGradient.Evaluate(healthPercentage);
    }
}
