using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

public class TempMusicScript : MonoBehaviour
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
        AudioManager.instance.SetMusic(MusicEnum.Ruins);

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


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentMusic.setParameterByName("HoMAdaptive", 0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            currentMusic.setParameterByName("HoMAdaptive", 1);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            AudioManager.instance.SetMusic(MusicEnum.Garden);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            AudioManager.instance.SetMusic(MusicEnum.Mirror);
        }
    }
}
