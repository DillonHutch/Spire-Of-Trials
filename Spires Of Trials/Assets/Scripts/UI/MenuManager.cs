using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu functionality, including starting and quitting the game.
/// </summary>
public class MenuManager : MonoBehaviour
{
    #region UnityMethods

    /// <summary>
    /// Called when the script starts.
    /// Currently unused but can be expanded for menu initialization.
    /// </summary>
    void Start()
    {
        // Future menu setup can be placed here if needed
    }

    /// <summary>
    /// Called once per frame.
    /// Currently unused but can be used for UI animations or updates.
    /// </summary>
    void Update()
    {
        // Potential UI updates or animations can be handled here
    }

    #endregion

    #region MenuFunctions

    /// <summary>
    /// Starts the game by loading the "Ruins" scene.
    /// </summary>
    public void StartGame()
    {
        RoundManager.ROUND_NUMBER = 0;
        SceneManager.LoadScene("RPGtestScreen"); // Loads the game scene
        
    }

    /// <summary>
    /// Exits the application.
    /// This will only work in a built application, not in the Unity editor.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit(); // Closes the game
        // Debug.Log("Quit Game called! (This will not work in the Unity editor)");
    }


    public void Credit()
    {
        SceneManager.LoadScene("WinScreen"); // Closes the game
        // Debug.Log("Quit Game called! (This will not work in the Unity editor)");
    }


    public void PlayHoverSound()
    {

        AudioManager.instance.PlayOneShot(FMODEvents.instance.menuHover, transform.position);
    }


    public void PlayClickSound()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.menuClick, transform.position);
    }



    public void PlayStartSound()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.menuStart, transform.position);
    }

    public void RestartGame()
    {
        EventManager.Instance.TriggerEvent("LoadNextLevel", "Ruins");
        RoundManager.ROUND_NUMBER = 0;
    }

    public void BackToMainMenu()
    {
        EventManager.Instance.TriggerEvent("LoadNextLevel", "MainMenu");
        RoundManager.ROUND_NUMBER = 0;
    }

    #endregion
}
