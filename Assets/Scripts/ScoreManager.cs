using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private const string HighScoreKey = "HighScoreLeaderboard";

    [System.Serializable]
    public class EnemyPoints
    {
        public string enemyTag;
        public int points;
    }

    [SerializeField] private List<EnemyPoints> enemyPointsList = new List<EnemyPoints>();
    private Dictionary<string, int> enemyPointsDict;

    private int levelScore = 0;
    private int totalScore = 0;

    [System.Serializable]
    private class ScoreData
    {
        public List<int> scores = new List<int>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        enemyPointsDict = new Dictionary<string, int>();
        foreach (var ep in enemyPointsList)
        {
            if (!enemyPointsDict.ContainsKey(ep.enemyTag))
            {
                enemyPointsDict.Add(ep.enemyTag, ep.points);
            }
        }
    }

    public void AddScoreToLeaderboard()
    {
        string json = PlayerPrefs.GetString(HighScoreKey, "{}");
        ScoreData data = JsonUtility.FromJson<ScoreData>(json);

        data.scores.Add(totalScore);

        data.scores = data.scores.OrderByDescending(s => s).ToList();

        if (data.scores.Count > 10)
        {
            data.scores = data.scores.GetRange(0, 10);
        }

        string updatedJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(HighScoreKey, updatedJson);
        PlayerPrefs.Save();
    }

    public List<int> GetLeaderboardScores()
    {
        string json = PlayerPrefs.GetString(HighScoreKey, "{}");
        ScoreData data = JsonUtility.FromJson<ScoreData>(json);
        return data.scores;
    }


    public void AddPointsForEnemy(string enemyTag)
    {
        if (enemyPointsDict.TryGetValue(enemyTag, out int pts))
        {
            levelScore += pts;
            totalScore += pts;
            Debug.Log($"Added {pts} points for {enemyTag}. Level Score: {levelScore}, Total Score: {totalScore}");
        }
        else
        {
            Debug.LogWarning($"No point value assigned for enemy tag: {enemyTag}");
        }
    }
    public int GetLevelScore() => levelScore;
    public int GetTotalScore() => totalScore;
    public void ResetLevelScore() => levelScore = 0;

    public void ResetTotalScore()
    {
        AddScoreToLeaderboard();
        totalScore = 0;
    }

    private void OnApplicationQuit()
    {
        AddScoreToLeaderboard();
    }
}