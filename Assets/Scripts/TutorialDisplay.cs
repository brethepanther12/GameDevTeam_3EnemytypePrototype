using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    private void Start()
    {
        if (canvas != null)
            canvas.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
            canvas.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
            canvas.enabled = false;
    }
}
