using UnityEngine;
using TMPro;

public class DifficultyDropdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        // Sync dropdown with saved/current difficulty
        if (gamemanager.instance != null)
        {
            dropdown.value = (int)gamemanager.instance.currentDifficulty;
        }

        // Add listener
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        if (gamemanager.instance != null)
        {
            gamemanager.instance.SetDifficultyByIndex(index);
        }
    }
}