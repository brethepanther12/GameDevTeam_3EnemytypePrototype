using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public Animator gateAnimator;
    private bool hasOpened = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasOpened) return;

        if (other.CompareTag("Player"))
        {
            gateAnimator.Play("BossGate");  // Use "BossGate" since that's animation state's name
            hasOpened = true;
        }
    }
}