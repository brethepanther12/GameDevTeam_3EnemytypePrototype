using UnityEngine;

public class GravityBehavior : MonoBehaviour
{

    private float storedGravity;
    public float gravityInVolume = -2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerController controller = other.GetComponent<playerController>();
        if (controller != null)
        {
            storedGravity = controller.GetGravity();
            controller.SetGravity(gravityInVolume);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerController controller = other.GetComponent<playerController>();
        if (controller != null)
        {
            controller.SetGravity(storedGravity);
        }
    }
}
