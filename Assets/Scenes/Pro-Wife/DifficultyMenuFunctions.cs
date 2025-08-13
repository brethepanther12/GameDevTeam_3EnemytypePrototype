using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DifficultyMenuFunctions : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown difficultyDropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshDropdown();
    }

    public void RefreshDropdown()
    {
        difficultyDropdown.ClearOptions();

        List<string> options = new List<string>();

        gamemanager.DifficultyLevels[] difficulties = new gamemanager.DifficultyLevels[]
        {
            gamemanager.DifficultyLevels.easy,
            gamemanager.DifficultyLevels.normal,
            gamemanager.DifficultyLevels.hard
        };

        for (int i = 0; i < difficulties.Length; i++)
        {
            gamemanager.DifficultyLevels difficulty = difficulties[i];

            bool locked = gamemanager.instance.IsDifficultyLocked(difficulty);

            string label = difficulty.ToString();

            if (locked)
            {
                label = label + " (Locked)";
            }

            options.Add(label);
        }

        difficultyDropdown.AddOptions(options);
        difficultyDropdown.value = (int)gamemanager.instance.currentDifficulty;
        difficultyDropdown.RefreshShownValue();
    }

    public void OnDifficultyChanged(int index)
    {
        gamemanager.instance.SetDifficulty((gamemanager.DifficultyLevels)index);
        RefreshDropdown();
    }
}
