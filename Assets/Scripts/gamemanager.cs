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
    [SerializeField] public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuOptions;
    [SerializeField] OptionsMenuUI optionMenuUI;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuInventory;
    [SerializeField] TMP_Text EnemiesRemaining;

    [SerializeField] private string nextLevelName;
    public int levelNumber;

    public static int playerDeathCount;
    public Image playerHPBar;
    public Image playerShieldBar;
    public Image playerArmorBar;
    public GameObject HurtImage;
    public GameObject ShieldBreak;
    public GameObject ArmorBreak;
    public GameObject playerDamagePanel;
    public GameObject playerShieldDamagePanel;
    public GameObject playerArmorDamagePanel;
    public GameObject PlayerSpawnPOS;
    public GameObject checkpointReached;

    public bool isPaused;
    public GameObject player;
    public playerController playerScript;

    public TMP_Text dashCounter;
    public TMP_Text dashCounterText;
    public Image dashCounterCDImage;

    public TMP_Text playerHp;
    public TMP_Text playerShield;
    public TMP_Text playerArmor;
    public TMPro.TextMeshProUGUI ammoText;
    public TMP_Text mutagenCountText;
    public TMP_Text componentCountText;
    public TMP_Text inventoryAmmo;
    public TMP_Text redKey;
    public TMP_Text blueKey;
    public TMP_Text yellowKey;
    public TMP_Text gunName;
    public TMP_Text fireModeText;
    public GameObject BossHealthBarUI;
    public Image BossHealthBarFill;
    public TMPro.TextMeshProUGUI BossNameText;
    public static event Action<DifficultyLevels> OnDifficultyChanged;


    float timescaleOrig;

    int gameGoalCount;

    public enum DifficultyLevels { easy, normal, hard }
    public DifficultyLevels currentDifficulty;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main Menu")
        {
            //Debug.Log("Game scene loaded. Finding references...");

            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerScript = player.GetComponent<playerController>();
            }

            PlayerSpawnPOS = GameObject.FindWithTag("Player Spawn POS");

            GameObject mainCanvas = GameObject.Find("UI (v4)");
            if (mainCanvas != null)
            {
                Transform hpBarTransform = mainCanvas.transform.Find("PlayerHPBarBackground/PlayerHPBar");
                if (hpBarTransform != null)
                {
                    playerHPBar = hpBarTransform.GetComponent<Image>();
                }

                EnemiesRemaining = GameObject.FindWithTag("EnemiesRemainingText").GetComponent<TMP_Text>();
            }

            Time.timeScale = 1f;
        }
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            ClearCheckpointData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        timescaleOrig = Time.timeScale;
        PlayerSpawnPOS = GameObject.FindWithTag("Player Spawn POS");
        currentDifficulty = DifficultyLevels.easy;
        loadDifficulty();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive != null)
            {
                CloseActiveMenu();
            }
            else
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

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

        if (menuActive.GetComponent<WeaponUIController>() != null)
        {

            menuActive.GetComponent<WeaponUIController>().CloseMenu();
        }
        else
        {
            menuActive.SetActive(false);
        }

        menuActive = null;
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        EnemiesRemaining.text = gameGoalCount.ToString("F0");
        if (gameGoalCount <= 0)
        {
            int highestLevelUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);

            if (levelNumber + 1 > highestLevelUnlocked)
            {
                PlayerPrefs.SetInt("LevelsUnlocked", levelNumber + 1);
                PlayerPrefs.Save();
            }

            PlayerPrefs.SetString("NextLevelToLoad", nextLevelName);
            PlayerPrefs.Save();

            SceneManager.LoadScene("LevelSummary");
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

        //You had this line before pause, but I couldn't die
        ScoreManager.instance.AddScoreToLeaderboard();

        playerDeathCount++;
        //Debug.Log("Player death count:" + playerDeathCount);
    }

    public void TriggerWinScreen()
    {
        int highestLevelUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);

        if (levelNumber + 1 > highestLevelUnlocked)
        {
            PlayerPrefs.SetInt("LevelsUnlocked", levelNumber + 1);
            PlayerPrefs.Save();
            //Debug.Log("Final level complete! Progress saved.");
        }

        unlockNextDifficulty(currentDifficulty);
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void UpdateBossHealthBar(int currentHP, int maxHP)
    {
        if (BossHealthBarFill != null)
        {
            BossHealthBarFill.fillAmount = (float)currentHP / maxHP;
        }
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

        int mutagenCount = inventory.GetMutagenCount();

        if (mutagenCountText != null)
            mutagenCountText.text = mutagenCount.ToString();

        int componentCount = inventory.GetWeaponComponentCount();

        if (componentCountText != null)
        {
            componentCountText.text = componentCount.ToString();
        }

    }


    public void OpenOptionsFromMainMenu()
    {
        if (menuOptions != null && optionMenuUI != null)
        {
            optionMenuUI.InitializeOptions();
            menuOptions.SetActive(true);
            menuActive = menuOptions;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
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
        if (!IsDifficultyLocked(difficulty))
        {
            currentDifficulty = difficulty;
            PlayerPrefs.SetInt("CurrentDifficulty", (int)difficulty);
            PlayerPrefs.Save();
            //Debug.Log($"Difficulty set to {difficulty}");

            // NOTIFY listeners (spawners, UI, etc.)
            OnDifficultyChanged?.Invoke(currentDifficulty);
        }
        else
        {
            //Debug.LogWarning($"{difficulty} is locked");
        }
    }

    private void loadDifficulty()
    {
        int saved = PlayerPrefs.GetInt("CurrentDifficulty", 0);
        currentDifficulty = (DifficultyLevels)saved;

        // Notify about the loaded difficulty
        OnDifficultyChanged?.Invoke(currentDifficulty);
    }

    public void ClearCheckpointData()
    {
        if (PlayerPrefs.HasKey("CheckpointPlayerData"))
        {
            PlayerPrefs.DeleteKey("CheckpointPlayerData");
            Debug.Log("Checkpoint data cleared for new game.");
        }
    }

    public void OpenMenu(GameObject menuToOpen)
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
        }

        statePause();
        menuActive = menuToOpen;
        menuActive.SetActive(true);
    }

    public void CloseActiveMenu()
    {
        if (menuActive == null) return;

        menuActive.SetActive(false);

        isPaused = false;
        Time.timeScale = timescaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive = null;
    }
    public void SetDifficultyByIndex(int index)
    {
        SetDifficulty((DifficultyLevels)index);
    }

    public void ResetGameState()
    {
        //Debug.Log("--- RESETTING GAME STATE ---");

        isPaused = false;
        gameGoalCount = 0;
        menuActive = null;

        gamemanager.playerDeathCount = 0;

        player = null;
        playerScript = null;
        PlayerSpawnPOS = null;
    }

    public void RespawnPlayer()
    {

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;

        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }

        if (playerScript != null)
        {
            playerScript.spawnPlayer();
        }
        else
        {
            //Debug.LogError("Could not find player script to respawn!");
        }
    }
}
