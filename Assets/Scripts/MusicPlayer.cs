using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip gameMusic;
    [SerializeField] AudioClip levelOne;
    [SerializeField] AudioClip bossFight;

    AudioSource audioSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (scene.name == "Tutorial")
        {
            PlayMusic(gameMusic);
        }
        else if (scene.name == "level 1")
        {
            PlayMusic(levelOne);
        }
        else if( scene.name == "Boss Fight")
        {
            PlayMusic(bossFight);
        }
        
    }

    void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip)
            return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}