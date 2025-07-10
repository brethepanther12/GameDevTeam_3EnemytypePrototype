using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text EnemiesRemaining;

    public Image playerHPBar;
    public Image playerShieldBar;
    public Image playerArmorBar;
    public GameObject playerDamagePanel;
    public GameObject playerShieldDamagePanel;
    public GameObject playerArmorDamagePanel;

    public bool isPaused;
    public GameObject player;
    public playerController playerScript;

    public TMPro.TextMeshProUGUI keyText; 
    public TMPro.TextMeshProUGUI ammoText;

    float timescaleOrig;

    int gameGoalCount;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        timescaleOrig = Time.timeScale;
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

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void TriggerWinScreen()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }
}
