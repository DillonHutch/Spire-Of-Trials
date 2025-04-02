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
        if(Input.GetKeyDown(KeyCode.P))
        {
            EventManager.Instance.TriggerEvent("LoadNextLevel", "Garden");
            RoundManager.ROUND_NUMBER = 40;
        }

        //Debug.LogWarning(RoundManager.ROUND_NUMBER);
    }
}
