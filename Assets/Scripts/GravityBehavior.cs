using UnityEngine;

public class GravityBehavior : MonoBehaviour
{

    public playerController controller;
    private float storedGravity;
    public float gravityInVolume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller == null)
        {
            controller = gamemanager.instance.playerScript;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            storedGravity = controller.GetGravity();
        }

        controller.SetGravity(gravityInVolume);
    }

    private void OnTriggerExit(Collider other)
    {
        controller.SetGravity(storedGravity);
    }
}
