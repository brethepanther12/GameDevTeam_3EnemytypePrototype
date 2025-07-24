using UnityEngine;

public class KillFloor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player.position = respawnPoint.position;
        }
       
    }
}
