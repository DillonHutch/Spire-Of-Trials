using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel; // Assign in the inspector
    [SerializeField] private GameObject volumePanel; // Assign in the inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Press Escape to close all and resume
        {
            if (volumePanel.activeSelf)
            {
                volumePanel.SetActive(false);
                ResumeGame();
            }
            else if (pausePanel.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void TogglePause()
    {
        if (pausePanel.activeSelf)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f; // Pause the game
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        pausePanel.SetActive(false);
        volumePanel.SetActive(false); // Ensure volume panel is also closed
    }


    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f; // Resume the game
    }
}
