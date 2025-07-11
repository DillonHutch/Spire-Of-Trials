using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitPuzzleController : MonoBehaviour
{
    [Tooltip("The obstacle to disable when both switches are pressed")]
    [SerializeField] private GameObject objectToDisable;

    // We'll assume exactly two switches, indexed 0 and 1:
    private bool[] switchPressed = new bool[2];

    // Called by each SwitchTrigger
    public void UpdateSwitchState(int switchIndex, bool isPressed)
    {
        switchPressed[switchIndex] = isPressed;
        CheckPuzzleComplete();
    }

    private void CheckPuzzleComplete()
    {
        // when both true, disable the obstacle
        if (switchPressed[0] && switchPressed[1])
        {
            if (objectToDisable != null)
                objectToDisable.SetActive(false);
            else
                Debug.LogWarning("PuzzleController: no objectToDisable assigned", this);

            // optionally disable further updates
            enabled = false;
        }
    }
}
