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
        public Button levelButton;
    }

    public List<LevelEntry> levelEntries;

    void Start()
    {
        foreach (var entry in levelEntries)
        {
            entry.levelButton.interactable = true;

            string levelName = entry.sceneName;

            entry.levelButton.onClick.AddListener(() => LoadLevel(entry.sceneName));
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