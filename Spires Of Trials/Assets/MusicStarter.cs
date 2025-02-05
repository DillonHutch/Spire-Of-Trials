using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class MusicStarter : MonoBehaviour
{
    private EventInstance currentMusic;

    void Start()
    {
        StartCoroutine(WaitForMusicInstance());
    }

    IEnumerator WaitForMusicInstance()
    {
        yield return new WaitUntil(() => AudioManager.instance != null);

        // Ensure music is set
        AudioManager.instance.SetMusic(MusicEnum.Title);

        // Get the current music instance
        currentMusic = AudioManager.instance.GetCurrentMusicInstance();

        if (currentMusic.isValid())
        {
            Debug.Log("Successfully retrieved valid music instance.");
            currentMusic.setParameterByName("HoMAdaptive", 0);



        }
        else
        {
            Debug.LogError("Current music instance is still invalid.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
