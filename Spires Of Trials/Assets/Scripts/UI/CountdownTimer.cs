using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages a countdown timer for the game session.
/// When the timer reaches zero, the player is redirected to the Main Menu.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    #region Timer Variables

    /// <summary>
    /// The total time remaining in seconds (initialized to 10 minutes).
    /// </summary>
    private float timeRemaining = 600f; // 10 minutes in seconds

    /// <summary>
    /// Reference to the UI text component that displays the countdown timer.
    /// </summary>
    [SerializeField] private TextMeshProUGUI countdownText;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called once per frame.
    /// Decreases the time remaining and updates the timer display.
    /// Ends the game and returns to the main menu when the timer reaches zero.
    /// </summary>
    private void Update()
    {
        if (timeRemaining > 0)
        {
            // Reduce the remaining time by the time elapsed since the last frame
            timeRemaining -= Time.deltaTime;

            // Update the countdown display
            UpdateTimerDisplay();
        }
        else
        {
            // Ensure the timer doesn't go below zero
            timeRemaining = 0;

            // Display "Time's Up!" message
            countdownText.text = "Time's Up!";

            // Load the Main Menu scene when the timer reaches zero
            SceneManager.LoadScene("MainMenu");
        }
    }

    #endregion

    #region Timer Display

    /// <summary>
    /// Updates the UI text to display the remaining time in MM:SS format.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        // Convert time remaining into minutes and seconds
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // Update the countdown text with the formatted time
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #endregion
}
