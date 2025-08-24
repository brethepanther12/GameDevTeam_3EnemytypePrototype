using UnityEngine;

public class ItemPickup : MonoBehaviour, IGrapplable
{
    [SerializeField] public Rigidbody rb;
    [SerializeField] private AudioClip pickupSound;
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
        if (inventory == null) return;

        playerController pc = gamemanager.instance.playerScript;

        if (weaponToGive != null)
        {
            inventory.AddWeapon(weaponToGive);
            GlobalInventory.instance.AddWeapon(weaponToGive);
            Debug.Log($"Picked up weapon: {weaponToGive.weaponName}");
        }

        if (itemToGive != null)
        {
            inventory.AddItem(itemToGive);
            Debug.Log($"Picked up item: {itemToGive.itemName}");

            if (itemToGive.itemName == "Weapon Component")
            {
                inventory.NotifyWeaponComponentsChanged();
            }
            else if (itemToGive.itemName == "Mutagen Sample")
            {
                inventory.UpdateMutagenDisplay();
            }
        }

        inventory.UpdateMutagenDisplay();
        inventory.UpdateComponentDisplay();

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }


        Destroy(gameObject);

        if (pc != null)
            pc.updatePlayerUI();
    }
}