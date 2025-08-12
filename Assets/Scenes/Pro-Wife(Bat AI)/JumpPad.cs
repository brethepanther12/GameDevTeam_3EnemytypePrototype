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
