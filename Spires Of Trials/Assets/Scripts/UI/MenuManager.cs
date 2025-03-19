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
        SceneManager.LoadScene("Ruins"); // Loads the game scene
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

    #endregion
}
