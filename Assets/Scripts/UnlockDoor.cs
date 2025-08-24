using UnityEngine;
using System.Collections.Generic;

public class UnlockDoor : MonoBehaviour
{
    public List<ItemSO> requiredItems;
    public GameObject doorObject;
    private bool isUnlocked = false;

    [SerializeField] private AudioClip lockedDoorSound;
    [SerializeField] private AudioClip unlockSound;

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked || !other.CompareTag("Player"))
            return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory != null && inventory.HasAllItems(requiredItems))
        {
            Unlock(inventory);
        }
        else
        {
          //  Debug.Log("You do not have the required items!");
        }
        if (lockedDoorSound != null)
        {
            AudioSource.PlayClipAtPoint(lockedDoorSound, transform.position);
        }
    }

    private void Unlock(PlayerInventory inventory)
    {
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }

        isUnlocked = true;

        foreach (ItemSO item in requiredItems)
        {

            inventory.ConsumeKey(item);
        }

        Destroy(doorObject);
    }
}
