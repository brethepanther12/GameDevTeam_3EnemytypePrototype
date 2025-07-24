using UnityEngine;

public class ItemPickup : MonoBehaviour, IGrapplable
{
    [SerializeField] private Rigidbody rb;

    public ItemSO itemToGive;
    public WeaponSO weaponToGive;

    public bool isBeingGrappled { get; set; }

    public bool canBeGrappled => true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
            return;

        playerController pc = gamemanager.instance.playerScript;

        if (weaponToGive != null)
        {
            inventory.AddWeapon(weaponToGive);
            GlobalInventory.instance.AddWeapon(weaponToGive);
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