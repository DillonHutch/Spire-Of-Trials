using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

/// <summary>
/// is audio manager
/// </summary>
public class AudioManager : MonoBehaviour
{


    #region Volume Controls

    [Header("Volume Settings")]

    [Range(0, 1)] public float masterVolume = 1;   // Controls overall game volume
    [Range(0, 1)] public float ambianceVolume = 1; // Controls ambiance/background noise volume
    [Range(0, 1)] public float musicVolume = 1;    // Controls background music volume
    [Range(0, 1)] public float sfxVolume = 1;      // Controls sound effects (SFX) volume

    #endregion

    #region FMOD Audio Instances

    /// <summary>
    /// Instance for the currently playing ambiance track.
    /// </summary>
    private EventInstance ambienceEvent;

    /// <summary>
    /// Instance for the currently playing music track.
    /// </summary>
    private EventInstance musicEventInstance;

    /// <summary>
    /// List to track active event instances, allowing for proper cleanup.
    /// </summary>
    private List<EventInstance> eventInstances = new List<EventInstance>();

    #endregion

    #region FMOD Audio Buses

    /// <summary>
    /// Master bus to control overall volume in FMOD.
    /// </summary>
    private Bus masterBus;

    /// <summary>
    /// Music bus to control music volume in FMOD.
    /// </summary>
    private Bus musicBus;

    /// <summary>
    /// Ambiance bus to control ambiance/background sound volume in FMOD.
    /// </summary>
    private Bus ambianceBus;

    /// <summary>
    /// Sound effects (SFX) bus to control all in-game sound effects in FMOD.
    /// </summary>
    private Bus sfxBus;

    #endregion

    #region Singleton Instance

    /// <summary>
    /// Singleton instance of the AudioManager to ensure only one exists in the game.
    /// </summary>
    public static AudioManager instance { get; private set; }

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern to ensure only one AudioManager exists.
    /// Initializes FMOD buses and prevents this instance from being destroyed on scene changes.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Destroy this instance if another instance already exists
            Destroy(this.gameObject);
            // Debug.LogError("Found more than one AudioManager in the scene.");
            return; // Exit to prevent further initialization
        }

        // Assign this instance as the singleton instance
        instance = this;

        // Prevent this instance from being destroyed when loading new scenes
        DontDestroyOnLoad(this.gameObject);

        // Initialize the list to track active FMOD event instances
        eventInstances = new List<EventInstance>();

        // Retrieve and assign FMOD audio buses for volume control
        masterBus = RuntimeManager.GetBus("bus:/");         // Master volume bus
        ambianceBus = RuntimeManager.GetBus("bus:/Ambiance"); // Ambiance sounds bus
        musicBus = RuntimeManager.GetBus("bus:/Music");       // Background music bus
        sfxBus = RuntimeManager.GetBus("bus:/SFX");         // Sound effects (SFX) bus
    }

    /// <summary>
    /// Called when the script starts.
    /// Ensures that music and ambiance are initialized if the FMODEvents instance is available.
    /// </summary>
    void Start()
    {
        // Ensure this instance is active and FMODEvents is initialized before playing audio
        if (instance == this && FMODEvents.instance != null)
        {
            InitializeMusic(FMODEvents.instance.music);   // Start background music
            InitializeAmbience(FMODEvents.instance.ambience); // Start ambiance sounds
        }
        else
        {
            Debug.LogError("FMODEvents.instance is null! Ensure FMODEvents is set up properly.");
        }
    }

    /// <summary>
    /// Called once per frame.
    /// Updates FMOD bus volumes based on user settings.
    /// </summary>
    void Update()
    {
        masterBus.setVolume(masterVolume);     // Update master volume
        musicBus.setVolume(musicVolume);       // Update music volume
        sfxBus.setVolume(sfxVolume);           // Update sound effects (SFX) volume
        ambianceBus.setVolume(ambianceVolume); // Update ambiance volume
    }

    #endregion

    #region Music Control

    /// <summary>
    /// Sets the music parameter in FMOD to switch between different tracks.
    /// Ensures the music event instance is valid before attempting to set the parameter.
    /// </summary>
    /// <param name="music">The music type from the MusicEnum to be set.</param>
    public void SetMusic(MusicEnum music)
    {
        if (!musicEventInstance.isValid())
        {
            Debug.LogError("SetMusic called but musicEventInstance is not valid!");
            return;
        }

        //Debug.Log($"Setting music parameter: {music}");
        musicEventInstance.setParameterByName("Music", (float)music);
    }

    /// <summary>
    /// Retrieves the current active music instance.
    /// </summary>
    /// <returns>The current FMOD music EventInstance.</returns>
    public EventInstance GetCurrentMusicInstance()
    {
        return musicEventInstance;
    }

    #endregion

    #region Sound Playback

    /// <summary>
    /// Plays a one-shot sound effect at a specified world position.
    /// Ideal for sound effects that do not require looping or persistent instances.
    /// </summary>
    /// <param name="sound">The FMOD event reference for the sound effect.</param>
    /// <param name="worldPos">The world position where the sound should play.</param>
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    #endregion

    #region Ambience & Music Initialization

    /// <summary>
    /// Initializes and starts the ambiance sound using the provided FMOD event reference.
    /// </summary>
    /// <param name="ambienceReference">The FMOD event reference for ambiance.</param>
    void InitializeAmbience(EventReference ambienceReference)
    {
        ambienceEvent = CreateInstance(ambienceReference);
        ambienceEvent.start();
    }

    /// <summary>
    /// Initializes and starts the background music if it has not been initialized yet.
    /// Ensures that music does not restart if it is already playing.
    /// </summary>
    /// <param name="musicReference">The FMOD event reference for the music.</param>
    void InitializeMusic(EventReference musicReference)
    {
        if (musicEventInstance.isValid())
        {
           // Debug.Log("Music event already initialized.");
            return; // Avoid reinitializing if already playing
        }

        // Ensure the event is created properly
        musicEventInstance = RuntimeManager.CreateInstance(musicReference);

        if (musicEventInstance.isValid())
        {
            //Debug.Log("Music event initialized successfully.");
            musicEventInstance.start();
        }
        else
        {
            //Debug.LogError("Failed to initialize music event.");
        }
    }

    #endregion

    #region Audio Instance Management

    /// <summary>
    /// Creates an FMOD EventInstance and adds it to the tracking list.
    /// Used for managing persistent sounds that require cleanup.
    /// </summary>
    /// <param name="eventReference">The FMOD event reference to create.</param>
    /// <returns>The created EventInstance.</returns>
    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    /// <summary>
    /// Cleans up all active FMOD EventInstances by stopping and releasing them.
    /// Ensures proper memory management and prevents audio leaks.
    /// </summary>
    void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }

    #endregion


}
