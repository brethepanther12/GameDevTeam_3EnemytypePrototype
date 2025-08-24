using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private AudioClip checkpointSound;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gamemanager.instance.PlayerSpawnPOS.transform.position != transform.position)
        {
            gamemanager.instance.PlayerSpawnPOS.transform.position = transform.position;
            gamemanager.instance.PlayerSpawnPOS.transform.localRotation = transform.localRotation;

            if (checkpointSound != null)
            {
                AudioSource.PlayClipAtPoint(checkpointSound, transform.position);
            }

            SavePlayerState(other.GetComponent<playerController>());

            StartCoroutine(checkpointFeedback());
        }
    }

    void SavePlayerState(playerController pc)
    {
        if (pc == null) return;

        PlayerData data = new PlayerData();
        data.health = pc.HP;
        data.shield = pc.shield; 
        data.armor = pc.armor; 

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString("CheckpointPlayerData", json);
        PlayerPrefs.Save();

        Debug.Log("Player state saved at checkpoint!");
    }

    IEnumerator checkpointFeedback()
    {
        gamemanager.instance.checkpointReached.SetActive(true);
        yield return new WaitForSeconds(1f);
        gamemanager.instance.checkpointReached.SetActive(false);
    }
}