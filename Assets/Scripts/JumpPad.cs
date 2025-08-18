using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float yVelocityJump;

    private bool playerInPad;
    private playerController playerReference;

    private void Update()
    {
        if (playerInPad && Input.GetButtonDown("Jump"))
        {
            //Player Get methods here
            Vector3 vel = playerReference.GetVerticalVelocity();
            vel.y += yVelocityJump; 
            playerReference.SetVerticalVelocity(vel);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController pc = other.GetComponent<playerController>();
            if (pc != null)
            {
                playerInPad = true;
                playerReference = pc;
            }
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
