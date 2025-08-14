using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectPanel;

    [System.Serializable]
    public class LevelEntry
    {
        public string sceneName;
        public int levelNumberToUnlock;
        public Button levelButton;
        public GameObject lockIcon;
    }

    public List<LevelEntry> levelEntries;

    void Start()
    {
        int highestLevelUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);

        foreach (var entry in levelEntries)
        {
            bool isUnlocked = highestLevelUnlocked >= entry.levelNumberToUnlock;

            entry.levelButton.interactable = isUnlocked;
            if (entry.lockIcon != null)
            {
                entry.lockIcon.SetActive(!isUnlocked);
            }

            if (isUnlocked)
            {
                entry.levelButton.onClick.AddListener(() => LoadLevel(entry.sceneName));
            }
        }
    }

    void LoadLevel(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ShowLevelSelect()
    {
        levelSelectPanel.SetActive(true);
    }

    public void HideLevelSelect()
    {
        levelSelectPanel.SetActive(false);
    }
}