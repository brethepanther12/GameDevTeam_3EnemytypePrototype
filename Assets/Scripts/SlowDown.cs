using UnityEngine;

public class SlowDown : MonoBehaviour
{

    int slowAmt;
    int slowMagnitude;

   
    private void OnTriggerEnter(Collider other)
    {

        playerController pc = gamemanager.instance.playerScript;

        if (other.CompareTag("Player"))
        {
            pc.IncreaseSpeed(-3, 1);
        }
            
                
    }

    private void OnTriggerExit(Collider other)
    {
        playerController pc = gamemanager.instance.playerScript;

        if (other.CompareTag("Player"))
        {
            pc.IncreaseSpeed(3, 1);
        }

    }
}
