using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableBattleMenu : MonoBehaviour
{



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                EventManager.Instance.TriggerEvent("openFightMenu");
            }
        }
    }
}
