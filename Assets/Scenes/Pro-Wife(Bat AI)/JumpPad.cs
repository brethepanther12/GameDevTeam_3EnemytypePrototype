using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float yVelocityJump;

    private bool playerInPad;
    private playerController playerReference;

    private void Update()
    {
        if (playerInPad && Input.GetKeyDown("Jump"))
        {
            //Player Get methods here
            Vector3 vel = playerReference.GetVerticalVelocity();
            vel.y = yVelocityJump; 
            playerReference.SetVerticalVelocity(vel);
            Debug.Log("Jump pad enabled...");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        playerController pc = other.GetComponent<playerController>();
        if (pc != null)
        {
            playerInPad = true;
            playerReference = pc;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        playerController pc = other.GetComponent<playerController>();
        if (pc != null && pc == playerReference)
        {
            playerInPad = false;
            playerReference = null;
        }
    }

}
