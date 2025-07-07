using UnityEngine;

public class UnlockDoor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController pc = gamemanager.instance.playerScript;

        if (other.CompareTag("Player") && pc.HasKey())
        {
            pc.AddKey(-1);

            Destroy(gameObject);
        }
            
    }

}
