using UnityEngine;

public class Knockback : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerPushback>()?.ApplyPush(transform.forward);
        }
    }
}
