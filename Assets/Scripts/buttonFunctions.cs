using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public void resume()
    {
        gamemanager.instance.stateUnpause();
        if(pauseMenuUI != null)
        pauseMenuUI.SetActive(false);
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
    }

    public void quit()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void respawnPlayer()
    {
        gamemanager.instance.playerScript.spawnPlayer();
        gamemanager.instance.stateUnpause();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }
}

