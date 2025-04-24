using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game's pause functionality, allowing players to pause, resume,
/// adjust volume, or return to the main menu.
/// </summary>
public class PauseGameManager : MonoBehaviour
{
    #region UI Elements

    /// <summary>
    /// Reference to the pause menu panel. Assigned in the Unity Inspector.
    /// </summary>
    [SerializeField] private GameObject pausePanel;

    /// <summary>
    /// Reference to the volume settings panel. Assigned in the Unity Inspector.
    /// </summary>
    [SerializeField] private GameObject volumePanel;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called once per frame.
    /// Listens for the Escape key to toggle the pause menu or close volume settings.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Press Escape to toggle pause or close menus
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

    #endregion

    #region PauseControlMethods

    /// <summary>
    /// Toggles the pause state of the game.
    /// If already paused, it resumes. Otherwise, it pauses the game.
    /// </summary>
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

    /// <summary>
    /// Pauses the game and displays the pause menu.
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = 0f; // Stop time to pause the game
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// Resumes the game by restoring time flow and hiding pause-related UI.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume game time
        pausePanel.SetActive(false);
        volumePanel.SetActive(false); // Ensure volume panel is also closed when resuming
    }

    #endregion

    #region SceneManagement

    /// <summary>
    /// Loads the main menu scene and ensures the game is unpaused when switching scenes.
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the Main Menu scene
        Time.timeScale = 1f; // Ensure time is running when switching scenes
        RoundManager.ROUND_NUMBER = 0;
    }

    #endregion
}
