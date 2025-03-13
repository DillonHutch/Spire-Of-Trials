using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithScreenSize : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(1920, 1080); // Set your base resolution

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        AdjustScale();
    }

    void Update()
    {
        AdjustScale();
    }

    void AdjustScale()
    {
        float scaleX = Screen.width / referenceResolution.x;
        float scaleY = Screen.height / referenceResolution.y;
        float scaleFactor = Mathf.Min(scaleX, scaleY); // Maintain aspect ratio

        transform.localScale = initialScale * scaleFactor;
    }
}
