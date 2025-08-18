using UnityEngine;

public class DroneBossTrigger : MonoBehaviour
{
    public DroneBoss droneBoss; 

    private bool hasActivated = false;

    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated) return;
        if (other.CompareTag("Player") && droneBoss != null)
        {
            hasActivated = true;
            droneBoss.ActivateBoss();
        }
    }
}