using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the end-of-game UI, allowing players to return to the main menu or quit the game.
/// </summary>
public class EndScript : MonoBehaviour
{
    #region UnityMethods

    /// <summary>
    /// Called when the script starts.
    /// Currently unused but can be expanded for end-screen setup.
    /// </summary>
    private void Start()
    {
        
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

    #region EndScreenActions

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the Main Menu scene
    }

    /// <summary>
    /// Quits the application.
    /// This only works in a built application and not in the Unity Editor.
    /// </summary>
    public void Quit()
    {
        Application.Quit(); // Close the application
        // Debug.Log("Quit called! (This does not work in the Unity Editor)");
    }

    #endregion
}
