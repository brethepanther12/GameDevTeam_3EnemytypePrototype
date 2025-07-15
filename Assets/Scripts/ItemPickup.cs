using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemSO itemToGive;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory != null && itemToGive != null)
        {
            playerController pc = gamemanager.instance.playerScript;

            inventory.AddItem(itemToGive);
            Destroy(gameObject);

            pc.updatePlayerUI();
        }
    }
}