using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Fliker Torch Lights
/// </summary>
public class Flicker : MonoBehaviour
{
    #region Light Flicker Settings

    [Header("Light Component")]
    private Light2D flickerLight; // Reference to the Light2D component (should be assigned at runtime)

    [Header("Flicker Settings")]
    [SerializeField] private float minIntensity = 1.5f; // Minimum intensity of the light flicker
    [SerializeField] private float maxIntensity = 2f;   // Maximum intensity of the light flicker
    [SerializeField] private float flickerSpeed = 2f;   // Speed at which the flicker effect changes

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script starts.
    /// Ensures the Light2D component is assigned and starts the flicker effect.
    /// </summary>
    private void Start()
    {
        // If the Light2D component is not manually assigned, try to find it on the GameObject
        if (flickerLight == null)
            flickerLight = GetComponent<Light2D>();

        // Start the flicker effect coroutine
        StartCoroutine(FlickerEffect());
    }

    #endregion

    #region FlickerMethods

    /// <summary>
    /// Continuously varies the light intensity to create a flickering effect.
    /// Changes intensity within the defined min and max range at a set speed.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator FlickerEffect()
    {
        while (true)
        {
            // Randomly adjust the light intensity between the min and max values
            flickerLight.intensity = Random.Range(minIntensity, maxIntensity);

            // Wait for the specified flicker speed duration before changing intensity again
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    #endregion


}
