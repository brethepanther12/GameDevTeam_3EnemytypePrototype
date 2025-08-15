using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    private AbilityUIController abilityUIController;
    private GameObject interactPrompt;

    private bool playerIsNearby = false;

    void Start()
    {
        interactPrompt = GameObject.FindWithTag("InteractPrompt");
        if (interactPrompt == null)
        {
            Debug.LogError("Upgrade Station could not find the InteractPrompt! Make sure it has the correct tag.");
        }
        else
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerIsNearby && Input.GetButtonDown("Interact"))
        {
            Debug.Log("Interact pressed");

            if (AbilityUIController.instance != null)
            {
                AbilityUIController.instance.OpenMenu();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered my trigger: " + other.name);

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