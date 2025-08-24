using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class NarrativeSlide
{
    public Sprite artwork;
    [TextArea(3, 10)]
    public string narrativeText;
    public float fadeInTime = 1.0f;
    public float typingSpeed = 0.04f;
}

public class StorybookController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private TMP_Text narrativeText;
    [SerializeField] private GameObject continuePrompt;

    [Header("Story Content")]
    [Tooltip("The list of slides that make up the intro cutscene.")]
    [SerializeField] private List<NarrativeSlide> slides;
    [SerializeField] private string sceneToLoadAfter;

    private int currentSlideIndex = 0;
    private bool isWaitingForInput = false;

    public void StartStory()
    {
        Time.timeScale = 1f;

        cutsceneImage.color = new Color(1, 1, 1, 0);
        mainPanel.SetActive(true);
        continuePrompt.SetActive(false);
        currentSlideIndex = 0;

        StartCoroutine(ProcessSlide(slides[currentSlideIndex]));
    }

    void Update()
    {
        if (isWaitingForInput && Input.GetButtonDown("Jump"))
        {
            isWaitingForInput = false;
            currentSlideIndex++;

            if (currentSlideIndex < slides.Count)
            {
                StartCoroutine(ProcessSlide(slides[currentSlideIndex]));
            }
            else
            {
                EndStory();
            }
        }
    }

    private IEnumerator ProcessSlide(NarrativeSlide slide)
    {
        continuePrompt.SetActive(false);
        narrativeText.text = ""; 

        cutsceneImage.sprite = slide.artwork;
        yield return StartCoroutine(FadeImage(1f, slide.fadeInTime));

        yield return StartCoroutine(TypeText(slide.narrativeText, slide.typingSpeed));

        continuePrompt.SetActive(true);
        isWaitingForInput = true;
    }

    private IEnumerator TypeText(string text, float speed)
    {
        foreach (char letter in text.ToCharArray())
        {
            narrativeText.text += letter;
            yield return new WaitForSeconds(speed);
        }
    }

    private IEnumerator FadeImage(float targetAlpha, float duration)
    {
        float startAlpha = cutsceneImage.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            cutsceneImage.color = new Color(1, 1, 1, newAlpha);
            yield return null;
        }

        cutsceneImage.color = new Color(1, 1, 1, targetAlpha);
    }

    private void EndStory()
    {
        StartCoroutine(EndStoryAndLoadScene());
    }

    private IEnumerator EndStoryAndLoadScene()
    {

        Time.timeScale = 1f;

        if (gamemanager.instance != null)
        {
            gamemanager.instance.ResetGameState();
        }

        yield return StartCoroutine(FadeImage(0f, 1.0f));

        if (!string.IsNullOrEmpty(sceneToLoadAfter))
        {
            SceneManager.LoadScene(sceneToLoadAfter);
        }
    }
}