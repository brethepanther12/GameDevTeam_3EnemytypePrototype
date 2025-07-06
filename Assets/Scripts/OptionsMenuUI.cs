using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuUI : MonoBehaviour
{
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;

    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject previousMenu;

    [SerializeField] AudioSource musicSource;

    public void InitializeOptions()
    {
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.RemoveAllListeners();

       
        fullscreenToggle.isOn = Screen.fullScreen;

        masterVolumeSlider.value = AudioListener.volume;

        if (musicSource != null)
            musicVolumeSlider.value = musicSource.volume;

        setMasterVolume(masterVolumeSlider.value);
        setMusicVolume(musicVolumeSlider.value);

        fullscreenToggle.onValueChanged.AddListener(setFullscreen);
        masterVolumeSlider.onValueChanged.AddListener(setMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(setMusicVolume);
    }

    public void setFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        //Debug.Log("Fullscreen changed to: " + isFullscreen);
    }

    public void setMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void setMusicVolume(float volume)
    {
        if(musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void backToPrevMenu()
    {
        optionsMenu.SetActive(false);
        previousMenu.SetActive(true);
    }
}