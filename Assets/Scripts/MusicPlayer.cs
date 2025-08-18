using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    [SerializeField] List<AudioClip> mainMenuTracks;
    [SerializeField] List<AudioClip> gameplayTracks;
    [SerializeField] List<AudioClip> bossFightTracks;

    private AudioSource audioSource;
    private List<AudioClip> currentPlaylist = new List<AudioClip>();
    private List<AudioClip> shuffledPlaylist = new List<AudioClip>();

    public float Volume
    {
        get => audioSource != null ? audioSource.volume : 0.5f;
        set
        {
            if (audioSource != null)
                audioSource.volume = Mathf.Clamp01(value);
        }
    }

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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    void Start()
    {
        Volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        TriggerMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TriggerMusicForScene(scene.name);
    }

    void TriggerMusicForScene(string sceneName)
    {
        shuffledPlaylist.Clear();

        if (sceneName == "Main Menu")
            currentPlaylist = mainMenuTracks;
        else if (sceneName == "Travis-(Boss enemy)")
            currentPlaylist = bossFightTracks;
        else
            currentPlaylist = gameplayTracks;

        PlayNextTrack();
    }

    void PlayNextTrack()
    {
        if (currentPlaylist.Count == 0) return;

        if (shuffledPlaylist.Count == 0)
            shuffledPlaylist = currentPlaylist.OrderBy(a => Random.value).ToList();

        AudioClip clipToPlay = shuffledPlaylist.Last();
        shuffledPlaylist.RemoveAt(shuffledPlaylist.Count - 1);
        PlayMusic(clipToPlay);
    }

    void Update()
    {
        if (audioSource == null) return; 

        if (!audioSource.isPlaying && currentPlaylist.Count > 0)
            PlayNextTrack();
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
}