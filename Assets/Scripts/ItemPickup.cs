using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemSO itemToGive;
    public WeaponSO weaponToGive;

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
            return;

        playerController pc = gamemanager.instance.playerScript;

        if (weaponToGive != null)
        {
            inventory.AddWeapon(weaponToGive);
            Debug.Log($"Picked up weapon: {weaponToGive.weaponName}");
        }
        else if (itemToGive != null)
        {
            inventory.AddItem(itemToGive);
            Debug.Log($"Picked up item: {itemToGive.itemName}");
        }

        Destroy(gameObject);

        if (pc != null)
            pc.updatePlayerUI();
    }
}