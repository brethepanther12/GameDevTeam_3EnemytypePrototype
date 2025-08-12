using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float yVelocityJump;

    private bool playerInPad;
    private playerController playerReference;

    private void Start()
    {
        playerReference = GetComponent<playerController>();
    }

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

        if (playerReference != null)
        {
            playerInPad = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerReference != null)
        {
            playerInPad = false;
            playerReference = null;
        }
    }

}
