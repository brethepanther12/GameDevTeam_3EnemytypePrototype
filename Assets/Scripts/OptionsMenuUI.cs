using UnityEngine;
using UnityEngine.UI;
using TMPro; // <- added

public class OptionsMenuUI : MonoBehaviour
{
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;

    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject previousMenu;

    [SerializeField] AudioSource musicSource;

    // NEW: difficulty dropdown (TextMeshPro)
    [SerializeField] TMP_Dropdown difficultyDropdown;

    public void InitializeOptions()
    {
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.RemoveAllListeners();

        // NEW: remove listeners for dropdown and set current value without triggering the callback
        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.RemoveAllListeners();
            if (gamemanager.instance != null)
            {
                // try to set without sending change event
                difficultyDropdown.SetValueWithoutNotify((int)gamemanager.instance.currentDifficulty);
            }
        }

        fullscreenToggle.isOn = Screen.fullScreen;
        masterVolumeSlider.value = AudioListener.volume;

        if (MusicPlayer.instance != null)
            musicVolumeSlider.value = MusicPlayer.instance.Volume;
        else if (musicSource != null)
            musicVolumeSlider.value = musicSource.volume;

        setMasterVolume(masterVolumeSlider.value);
        setMusicVolume(musicVolumeSlider.value);

        fullscreenToggle.onValueChanged.AddListener(setFullscreen);
        masterVolumeSlider.onValueChanged.AddListener(setMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(setMusicVolume);

        // NEW: add dropdown listener (after initial SetValueWithoutNotify)
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
    }

    // NEW: callback for difficulty dropdown
    private void OnDifficultyDropdownChanged(int index)
    {
        if (gamemanager.instance != null)
        {
            gamemanager.instance.SetDifficultyByIndex(index);
        }
    }

    public void setFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void setMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void setMusicVolume(float volume)
    {
        if (MusicPlayer.instance != null)
        {
            MusicPlayer.instance.Volume = volume;
        }
        else if (musicSource != null)
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