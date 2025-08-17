using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TMP_Dropdown

public class OptionsMenuUI : MonoBehaviour
{
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;

    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject previousMenu;

    [SerializeField] AudioSource musicSource;

    // Difficulty dropdown (TextMeshPro)
    [SerializeField] TMP_Dropdown difficultyDropdown;

    public void InitializeOptions()
    {
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.RemoveAllListeners();

        if (difficultyDropdown != null)
        {
            difficultyDropdown.onValueChanged.RemoveAllListeners();
            if (gamemanager.instance != null)
            {
                // Set dropdown value without triggering callback
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

        // Add dropdown listener after initial value set
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
    }

    // Callback for difficulty dropdown
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