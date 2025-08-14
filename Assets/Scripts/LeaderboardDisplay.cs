using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform scoreContainer;
    [SerializeField] private GameObject scoreEntryPrefab;

    void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        foreach (Transform child in scoreContainer)
        {
            Destroy(child.gameObject);
        }

        List<int> scores = ScoreManager.instance.GetLeaderboardScores();

        for (int i = 0; i < scores.Count; i++)
        {
            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreContainer);
            TMP_Text[] texts = entryObject.GetComponentsInChildren<TMP_Text>();
            texts[0].text = "#" + (i + 1); 
            texts[1].text = scores[i].ToString(); 
        }

        leaderboardPanel.SetActive(true);
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
}