using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSummaryController : MonoBehaviour
{
    [SerializeField] TMP_Text levelScoreText;
    [SerializeField] TMP_Text totalScoreText;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (ScoreManager.instance != null)
        {
            levelScoreText.text = "Level Score: " + ScoreManager.instance.GetLevelScore();
            totalScoreText.text = "Total Score: " + ScoreManager.instance.GetTotalScore();
        }
    }

    public void OnContinuePressed()
    {
        int summarySceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = summarySceneIndex + 1;

        ScoreManager.instance.ResetLevelScore();

        SceneManager.LoadScene(nextLevelIndex);
    }
}