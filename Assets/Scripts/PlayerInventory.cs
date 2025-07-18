using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<ItemSO> collectedItems = new List<ItemSO>();
    

    public void AddItem(ItemSO item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            Debug.Log("Picked up: " + item.itemName);

        }
        else if (collectedItems.Contains(item) && item.quantityToPickup + item.quantityHeld < item.stackSize)
        {
            item.quantityHeld += item.quantityToPickup;
        }

    }

    public bool HasAllItems(List<ItemSO> requiredItems)
    {
        foreach (ItemSO item in requiredItems)
        {
            if (!collectedItems.Contains(item))

             return false;
        }
        return true;
    }
    
    public bool HasItem(ItemSO item)
    {
        return collectedItems.Contains(item);
    }

    public int GetAmmoAmount(string ammoName)
    {
        foreach (ItemSO item in collectedItems)
        {
            if (item.itemName == ammoName)
            {
                return item.quantityHeld;
            }
        }
        return 0;
    }

    public void ConsumeAmmo(int amount)
    {
        foreach (ItemSO item in collectedItems)
        {
            if (item.itemName == "Ammo")
            {
                item.quantityHeld -= amount;
            }
        }
        
    }

}
