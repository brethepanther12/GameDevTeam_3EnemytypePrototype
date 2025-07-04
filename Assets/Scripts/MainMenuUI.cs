using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
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
        // this is a place holder
        Debug.Log("Options opened");
    }
 }
