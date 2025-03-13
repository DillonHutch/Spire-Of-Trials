using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flicker : MonoBehaviour
{
    private Light2D flickerLight; // Assign the Light component
    [SerializeField] private float minIntensity = 1.5f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float flickerSpeed = 2f;

    private void Start()
    {
        


        if (flickerLight == null)
            flickerLight = GetComponent<Light2D>();

        StartCoroutine(FlickerEffect());
    }

    private IEnumerator FlickerEffect()
    {
        while (true)
        {
            flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}
