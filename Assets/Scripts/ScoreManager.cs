using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

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
            enemyPointsDict[ep.enemyTag] = ep.points;
        }
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

    public void ResetTotalScore() => totalScore = 0;
}