using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{
    public AudioSource battleMusic;
    public AudioSource ambientMusic;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ambientMusic.Stop();
            battleMusic.Play();
            Destroy(gameObject);
        }
    }
}