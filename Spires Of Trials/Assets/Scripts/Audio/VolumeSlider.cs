using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles individual volume sliders in the settings menu.
/// Adjusts the corresponding volume type (Master, Music, Ambience, or SFX) in the AudioManager.
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    #region VolumeType Enum

    /// <summary>
    /// Enum representing different volume categories that can be adjusted.
    /// </summary>
    private enum VolumeType
    {
        MASTER,   // Controls overall game volume
        MUSIC,    // Controls background music volume
        AMBIANCE, // Controls ambiance/background noise volume
        SFX       // Controls sound effects (SFX) volume
    }

    #endregion

    #region Serialized Fields

    /// <summary>
    /// Determines which volume category this slider controls.
    /// Assigned in the Unity Inspector.
    /// </summary>
    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    /// <summary>
    /// Reference to the UI slider component for adjusting volume.
    /// </summary>
    private Slider volumeSlider;

    #endregion

    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Finds and assigns the Slider component from its children.
    /// </summary>
    private void Awake()
    {
        volumeSlider = this.GetComponentInChildren<Slider>();
    }

    /// <summary>
    /// Called once per frame.
    /// Updates the slider's value to match the corresponding volume setting.
    /// </summary>
    private void Update()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                volumeSlider.value = AudioManager.instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volumeSlider.value = AudioManager.instance.musicVolume;
                break;
            case VolumeType.AMBIANCE:
                volumeSlider.value = AudioManager.instance.ambianceVolume;
                break;
            case VolumeType.SFX:
                volumeSlider.value = AudioManager.instance.sfxVolume;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }

    #endregion

    #region VolumeAdjustment

    /// <summary>
    /// Called when the slider value is changed.
    /// Updates the corresponding volume setting in the AudioManager.
    /// </summary>
    public void OnSliderValueChanged()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                AudioManager.instance.masterVolume = volumeSlider.value;
                break;
            case VolumeType.MUSIC:
                AudioManager.instance.musicVolume = volumeSlider.value;
                break;
            case VolumeType.AMBIANCE:
                AudioManager.instance.ambianceVolume = volumeSlider.value;
                break;
            case VolumeType.SFX:
                AudioManager.instance.sfxVolume = volumeSlider.value;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }

    #endregion
}
