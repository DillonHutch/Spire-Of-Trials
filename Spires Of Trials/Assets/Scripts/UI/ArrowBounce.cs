//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ArrowBounce : MonoBehaviour
//{
//    public float floatSpeed = 10f; // Speed of floating motion
//    public float floatAmplitude = 10; // How much it floats up and down

//    private Vector3 startLocalPos;
//    private SpriteRenderer spriteRenderer;
//    private Renderer objectRenderer;

//    void Start()
//    {
//        startLocalPos = transform.localPosition;

//        // Check for SpriteRenderer or standard Renderer
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        objectRenderer = GetComponent<Renderer>();
//    }

//    void Update()
//    {
//        float alpha = GetAlpha();

//        if (alpha == 1f)
//        {
//            // Floating effect when visible
//            transform.localPosition = startLocalPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude, 0);
//        }
//        else if (alpha == 0f)
//        {
//            // Stop movement when invisible
//            transform.localPosition = startLocalPos;
//        }
//    }

//    private float GetAlpha()
//    {
//        if (spriteRenderer != null)
//        {
//            return spriteRenderer.color.a; // Get alpha from sprite color
//        }
//        else if (objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
//        {
//            return objectRenderer.material.color.a; // Get alpha from material color
//        }
//        return 1f; // Default to visible if no renderer is found
//    }
//}
