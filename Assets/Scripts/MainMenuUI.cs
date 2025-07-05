using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] OptionsMenuUI optionMenuUI;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject mainMenuPanel;

    public void startGame()
    {
        // I have this going to my scene for now for testing purposes.
        SceneManager.LoadScene("Jack Jowers Scene");
    }

    public void quitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void openOptions()
    {
        optionMenuUI.InitializeOptions();
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }
    
    public void closeOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
 }
