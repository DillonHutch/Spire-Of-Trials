using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SceneManager.LoadScene("Garden");
            RoundManager.ROUND_NUMBER = 49;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene("Sanctum");
            RoundManager.ROUND_NUMBER = 75;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            SceneManager.LoadScene("Ruins");
            RoundManager.ROUND_NUMBER = 24;
        }

        //Debug.LogWarning(RoundManager.ROUND_NUMBER);
    }
}
