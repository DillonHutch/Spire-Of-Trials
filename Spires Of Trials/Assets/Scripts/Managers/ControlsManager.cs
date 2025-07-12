using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsManager : MonoBehaviour
{

  


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Instance.TriggerEvent("submitPressed", EventManager.Instance.inputEventContext);
        }
    }
}
