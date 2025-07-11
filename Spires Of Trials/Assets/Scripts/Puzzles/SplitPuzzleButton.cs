using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitPuzzleButton : MonoBehaviour
{

    [Tooltip("Index into the PuzzleController's switch array (0 or 1)")]
    [SerializeField] private int switchIndex;

    [Tooltip("Drag your PuzzleController here")]
    [SerializeField] private SplitPuzzleController controller;


    string requiredTag = "OverworldPlayer";

    private bool isPressed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SwitchTrigger:{switchIndex}] TriggerEnter by '{other.name}' (tag='{other.tag}')");

        if (other.CompareTag(requiredTag))
        {
            Debug.Log($"[SwitchTrigger:{switchIndex}] Tag matched '{requiredTag}' → registering press");
            controller.UpdateSwitchState(switchIndex, true);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (isPressed && (other.CompareTag("OverworldPlayer") || other.CompareTag("GhostPlayer")))
        {
            isPressed = false;
            controller.UpdateSwitchState(switchIndex, false);
        }
    }
}
