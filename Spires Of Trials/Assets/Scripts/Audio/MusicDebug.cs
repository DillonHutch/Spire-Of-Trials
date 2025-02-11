using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicDebug : MonoBehaviour
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

    public void PlayTitleMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Title);
    }

    public void PlayRuinsSlowMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Ruins);
    }

    public void PlayRuinsFastMusic()
    {
        
    }

    public void PlayGardenSlowMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Garden);
    }

    public void PlayGardenFastMusic()
    {
        
    }


    public void PlayMirrorSlowMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Mirror);
        currentMusic.setParameterByName("HoMAdaptive", 0);
    }

    public void PlayMirrorFastMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Mirror);
        currentMusic.setParameterByName("HoMAdaptive", 1);
    }

    public void PlaySanctumSlowMusic()
    {
        AudioManager.instance.SetMusic(MusicEnum.Sanctum);
    }

    public void PlaySanctumFastMusic()
    {

    }


}
