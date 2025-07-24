using UnityEngine;
using System.Collections.Generic;

public class UnlockDoor : MonoBehaviour
{
    public List<ItemSO> requiredItems;
    public GameObject doorObject;
    private bool isUnlocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked)
            return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if(inventory != null && inventory.HasAllItems(requiredItems))
        {
            Unlock();
        }
        else
        {
            Debug.Log("You do not have the required items!");
        }
    }

    private void Unlock()
    {
        isUnlocked = true;
        PlayerInventory inventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
        foreach (ItemSO item in requiredItems)
        {
            if (inventory.collectedItems.Contains(item))
                inventory.collectedItems.Remove(item);
        }

        Destroy(doorObject);
    }
}
