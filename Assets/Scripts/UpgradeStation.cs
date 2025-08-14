using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The UI panel for the upgrade menu.")]
    [SerializeField] private AbilityUIController abilityUIController;

    [Tooltip("The UI text prompt that says 'Press E to Interact'.")]
    [SerializeField] private GameObject interactPrompt;

    private bool playerIsNearby = false;

    void Start()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerIsNearby && Input.GetButtonDown("Interact"))
        {
            Debug.Log("Opening upgrade menu...");
            abilityUIController.OpenMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}