using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSummaryController : MonoBehaviour
{
    [SerializeField] TMP_Text levelScoreText;
    [SerializeField] TMP_Text totalScoreText;

    private string nextLevelToLoad;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (ScoreManager.instance != null)
        {
            levelScoreText.text = "Level Score: " + ScoreManager.instance.GetLevelScore();
            totalScoreText.text = "Total Score: " + ScoreManager.instance.GetTotalScore();
        }

        nextLevelToLoad = PlayerPrefs.GetString("NextLevelToLoad");
    }

    public void OnContinuePressed()
    {
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetLevelScore();
        }

        if (!string.IsNullOrEmpty(nextLevelToLoad))
        {
            SceneManager.LoadScene(nextLevelToLoad);
        }
        else
        {
            if(ScoreManager.instance!= null)
            {
                ScoreManager.instance.ResetTotalScore();
            }

            SceneManager.LoadScene("Main Menu");
        }
    }
}