using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [Header("References")]
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    [Tooltip("Reference to player settings model")]
    public PlayerControllerScr player;

    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "hz";
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height &&
                resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("volume", Mathf.Log10(volume) * 20);
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen (bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void SetResolution (int resolutionIndex)
    {
        Resolution resoltion = resolutions[resolutionIndex];
        Screen.SetResolution(resoltion.width, resoltion.height, Screen.fullScreen);
    }

    public void SetViewHorizontal (float sensitivity)
    {
        player.playerSettings.ViewXSensitivity = sensitivity;
    }

    public void SetViewVertical (float sensitivity)
    {
        player.playerSettings.ViewYSensitivity = sensitivity;
    }

    public void SetViewHorizontalInverted(bool inverted)
    {
        player.playerSettings.ViewXInverted = inverted;
    }

    public void SetViewVerticalInverted(bool inverted)
    {
        player.playerSettings.ViewYInverted = inverted;
    }
}
