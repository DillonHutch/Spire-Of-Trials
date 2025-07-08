using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchingWorldsScript : MonoBehaviour
{



    [SerializeField] GameObject past;
    [SerializeField] GameObject present;
    [SerializeField] GameObject future;




    // Update is called once per frame
    void Update()
    {
        // Top-row “1” key
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            past.SetActive(true);
            present.SetActive(false);
            future.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            past.SetActive(false);
            present.SetActive(true);
            future.SetActive(false);
        }


        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            past.SetActive(false);
            present.SetActive(false);
            future.SetActive(true);
        }

    }
}
