using UnityEngine;
using System.Collections.Generic;

public class UnlockDoor : MonoBehaviour
{
    public List<ItemSO> requiredItems;
    public GameObject doorObject;
    private bool isUnlocked = false;

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
            Debug.Log("You do not have the required items!");
        }
    }

    private void Unlock(PlayerInventory inventory)
    {
        isUnlocked = true;

        foreach (ItemSO item in requiredItems)
        {

            inventory.ConsumeKey(item);
        }

        Destroy(doorObject);
    }
}
