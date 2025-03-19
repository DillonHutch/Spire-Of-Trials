using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

/// <summary>
/// Handles the initialization of background music when a scene starts.
/// Uses the AudioManager to set the appropriate music track.
/// </summary>
public class MusicStarter : MonoBehaviour
{

    #region Fields

    /// <summary>
    /// Stores the currently playing music event instance.
    /// </summary>
    private EventInstance currentMusic;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the scene starts.
    /// Sets the background music to the "Title" track using the AudioManager.
    /// </summary>
    void Start()
    {
        AudioManager.instance.SetMusic(MusicEnum.Title);
    }

    #endregion
}
