using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gamemanager.instance.PlayerSpawnPOS.transform.position != transform.position)
        {
            gamemanager.instance.PlayerSpawnPOS.transform.position = transform.position;
            gamemanager.instance.PlayerSpawnPOS.transform.localRotation = transform.localRotation;
            StartCoroutine(checkpointFeedback());
        }
    }

    IEnumerator checkpointFeedback()
    {
        gamemanager.instance.checkpointReached.SetActive(true);
        yield return new WaitForSeconds(1f);
        gamemanager.instance.checkpointReached.SetActive(false);
    }
}
