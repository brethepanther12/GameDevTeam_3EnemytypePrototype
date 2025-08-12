using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;
//using static System.Net.Mime.MediaTypeNames;
//using static UnityEditor.Progress;
public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuOptions;
    [SerializeField] OptionsMenuUI optionMenuUI;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuInventory;
    [SerializeField] TMP_Text EnemiesRemaining;

    public Image playerHPBar;
    public Image playerShieldBar;
    public Image playerArmorBar;
    public RawImage HurtImage;
    public GameObject playerDamagePanel;
    public GameObject playerShieldDamagePanel;
    public GameObject playerArmorDamagePanel;
    public GameObject PlayerSpawnPOS;
    public GameObject checkpointReached;

    public bool isPaused;
    public GameObject player;
    public playerController playerScript;

    public TMP_Text jumpCounter;
    public TMP_Text playerHp;
    public TMP_Text playerShield;
    public TMP_Text playerArmor;
    public TMPro.TextMeshProUGUI ammoText;
    public TMP_Text inventoryAmmo;
    public TMP_Text redKey;
    public TMP_Text blueKey;
    public TMP_Text yellowKey;
    public TMP_Text gunName;
    public TMP_Text fireModeText;
    public GameObject BossHealthBarUI;
    public Image BossHealthBarFill;
    public TMPro.TextMeshProUGUI BossNameText;

    public BossAI currentBoss;

    float timescaleOrig;

    int gameGoalCount;

    public enum DifficultyLevels{easy,normal,hard}
    public DifficultyLevels currentDifficulty;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        timescaleOrig = Time.timeScale;
        PlayerSpawnPOS = GameObject.FindWithTag("Player Spawn POS");
        currentDifficulty = DifficultyLevels.easy;
        loadDifficulty();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
        if (Input.GetButtonDown("Inventory"))
        {
            if (menuActive == null)
            {
                openInventory();
                updateInventoryUI();
            }
            else if (menuActive == menuInventory)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timescaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        EnemiesRemaining.text = gameGoalCount.ToString("F0");
        if (gameGoalCount <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void openInventory()
    {
        statePause();
        menuActive = menuInventory;
        menuActive.SetActive(true);
        updateInventoryUI();
    }
    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void TriggerWinScreen()
    {
        unlockNextDifficulty(currentDifficulty);
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void StartBossFight(BossAI boss)
    {
        currentBoss = boss;
        BossHealthBarUI.SetActive(true);
        BossNameText.text = boss.bossName;
    }
    public void UpdateBossHealthBar(int currentHP, int maxHP)
    {
        if (BossHealthBarFill != null)
        {
            BossHealthBarFill.fillAmount = (float)currentHP / maxHP;
        }
    }

    public void EndBossFight()
    {
        BossHealthBarUI.SetActive(false);
        currentBoss = null;
    }
    public void updateInventoryUI()
    {
        if (playerScript == null)
        {
            return;
        }
        PlayerInventory inventory = playerScript.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }
        int ammoCount = inventory.GetAmmoAmount("Ammo");
        inventoryAmmo.text = ammoCount.ToString();

        int redKeys = inventory.GetAmmoAmount("Red Key");
        redKey.text = redKeys.ToString();

        int blueKeys = inventory.GetAmmoAmount("Blue Key");
        blueKey.text = blueKeys.ToString();

        int yellowKeys = inventory.GetAmmoAmount("Yellow Key");
        yellowKey.text = yellowKeys.ToString();

    }

    public void openOptionsFromPause()
    {
        if (menuOptions != null && optionMenuUI != null)
        {
            optionMenuUI.InitializeOptions(); 
            menuPause.SetActive(false);
            menuOptions.SetActive(true);
            menuActive = menuOptions;
        }
    }

    public void closeOptionsFromPause()
    {
        menuOptions.SetActive(false);
        menuPause.SetActive(true);
        menuActive = menuPause;
    }

    public bool IsDifficultyLocked(DifficultyLevels difficulty)
    {
        if (difficulty == DifficultyLevels.easy)
        {
            return false;
        }

        string key = "DifficultyUnlocked_" + difficulty.ToString();
        return PlayerPrefs.GetInt(key, 0) == 0;
    }

    public void unlockDifficultyLevel(DifficultyLevels difficulty)
    {
        string key = "DifficultyUnlocked_" + difficulty.ToString();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
    private void unlockNextDifficulty(DifficultyLevels completed)
    {
        if (completed == DifficultyLevels.easy)
        {
            unlockDifficultyLevel(DifficultyLevels.normal);
        }
        else if (completed == DifficultyLevels.normal)
        {
            unlockDifficultyLevel(DifficultyLevels.hard);
        }
    }

    public void SetDifficulty(DifficultyLevels difficulty)
    {
        if(!IsDifficultyLocked(difficulty))
        {
            currentDifficulty = difficulty;
            PlayerPrefs.SetInt("CurrentDifficulty", (int)difficulty);
            PlayerPrefs.Save();
            Debug.Log($"Difficulty set to {difficulty}");
        }
        else
        {
            Debug.LogWarning($"{difficulty} is locked");
        }
    }

    private void loadDifficulty()
    {
        int saved = PlayerPrefs.GetInt("CurrentDifficulty", 0);
        currentDifficulty = (DifficultyLevels)saved;
    }
}
