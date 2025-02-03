using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{

    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]

    public float musicVolume = 1;
    [Range(0, 1)]
    public float sfxVolume = 1;
    [Range(0, 1)]


    private Bus masterBus;

    private Bus musicBus;

    private Bus sfxBus;


    List<EventInstance> eventInstances;

    public static AudioManager instance { get; private set; }


    private EventInstance musicEventInstance;



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Destroy this instance if another instance already exists
            Destroy(this.gameObject);
            Debug.LogError("Found more than one AudioManager in the scene.");
            return; // Exit to prevent further initialization
        }

        // Assign this instance as the singleton instance
        instance = this;

        // Prevent this instance from being destroyed on scene load
        DontDestroyOnLoad(this.gameObject);

        // Initialize lists and FMOD buses
        eventInstances = new List<EventInstance>();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }


    public void SetMusic(MusicEnum music)
    {
        musicEventInstance.setParameterByName("Music", (float)music);
    }


    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }


    void InitializeMusic(EventReference musicReference)
    {
        if (musicEventInstance.isValid())
            return; // Avoid reinitializing if already playing

        musicEventInstance = CreateInstance(musicReference);
        musicEventInstance.start();
    }



    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }





    // Start is called before the first frame update
    void Start()
    {
        if (instance == this)
        {
            InitializeMusic(FMODEvents.instance.music);
        }
    }


    // Update is called once per frame
    void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(sfxVolume);
    }
}
