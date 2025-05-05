using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenuButton();
        }


    }

    public void GoToMainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
        RoundManager.ROUND_NUMBER = 0;
    }
}
