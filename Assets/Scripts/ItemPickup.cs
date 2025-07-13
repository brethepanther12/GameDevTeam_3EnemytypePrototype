using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemSO itemToGive;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory != null && itemToGive != null)
        {
            inventory.AddItem(itemToGive);
            Destroy(gameObject);
        }
    }
}