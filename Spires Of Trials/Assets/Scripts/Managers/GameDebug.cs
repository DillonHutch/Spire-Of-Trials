using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            EventManager.Instance.TriggerEvent("LoadNextLevel", "Garden");
            RoundManager.ROUND_NUMBER = 50;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            EventManager.Instance.TriggerEvent("LoadNextLevel", "Sanctum");
            RoundManager.ROUND_NUMBER = 75;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            EventManager.Instance.TriggerEvent("LoadNextLevel", "Ruins");
            RoundManager.ROUND_NUMBER = 25;
        }

        //Debug.LogWarning(RoundManager.ROUND_NUMBER);
    }
}
