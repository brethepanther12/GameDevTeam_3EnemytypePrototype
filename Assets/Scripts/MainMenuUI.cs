using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] OptionsMenuUI optionMenuUI;
    [SerializeField] GameObject optionsPanel;

    [SerializeField] GameObject creditsPanel;
    [SerializeField] RectTransform creditsContainer;
    [SerializeField] float scrollSpeed;
    bool isScrolling;

    [SerializeField] GameObject mainMenuPanel;

    private float startY;   
    private float endY;    

    private void Start()
    {
        if (creditsContainer != null)
        {
            float screenHeight = ((RectTransform)creditsPanel.transform).rect.height;
            float creditsHeight = creditsContainer.rect.height;

            startY = -screenHeight;
            endY = creditsHeight;               
        }
    }

    private void Update()
    {
        if (isScrolling && creditsContainer != null)
        { 
            creditsContainer.anchoredPosition +=
                Vector2.up * scrollSpeed * Time.unscaledDeltaTime;

            if (creditsContainer.anchoredPosition.y >= endY)
            {
                closeCredits();
            }
        }
    }

    public void startGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void quitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void openOptions()
    {
        optionMenuUI.InitializeOptions();
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void closeOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void openCredits()
    {
        creditsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        isScrolling = true;

        creditsContainer.anchoredPosition = new Vector2(
            creditsContainer.anchoredPosition.x,
            startY
        );
    }

    public void closeCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        isScrolling = false;
    }
}