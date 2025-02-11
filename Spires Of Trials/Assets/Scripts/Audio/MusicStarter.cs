using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class MusicStarter : MonoBehaviour
{
    private EventInstance currentMusic;

    void Start()
    {
        AudioManager.instance.SetMusic(MusicEnum.Title);
    }

    
}
