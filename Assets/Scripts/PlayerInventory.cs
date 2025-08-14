using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int quantity;

    public InventorySlot(ItemSO item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
    }
}


public class PlayerInventory : MonoBehaviour
{
    [HideInInspector] public playerController playerRef;

    [Header("Item & Currency Inventory")]
    public List<InventorySlot> items = new List<InventorySlot>();

    [Header("Weapon Inventory")]
    public List<ItemSO> collectedItems = new List<ItemSO>();
    public List<WeaponSO> weaponInventory = new List<WeaponSO>();
    public WeaponSO equippedWeapon;
    public int weaponListPos = 0;

    [Header("Runtime References")]
    public GameObject weaponSocket;
    private GameObject currentWeaponInstance;
    private Weapon currentWeaponScript;

    [Header("UI References")]
    public TMP_Text mutagenCountText;


    private Dictionary<AmmoType, string> ammoLookup = new Dictionary<AmmoType, string>
    {

    { AmmoType.Pistol, "Pistol Bullets" },
    { AmmoType.AR, "Rifle Ammo" },
    { AmmoType.Shell, "Shotgun Shells" },
    { AmmoType.Grenade, "Frag Round" },
    { AmmoType.Rocket, "Rocket(Homing)" },
    



    };

    private void Start()
    {
        UpdateMutagenDisplay();
    }

    public void AddItem(ItemSO item)
    {
        InventorySlot slot = items.Find(s => s.item == item);

        if (slot != null)
        {
            int availableSpace = slot.item.stackSize - slot.quantity;
            int amountToAdd = Mathf.Min(item.quantityToPickup, availableSpace);
            slot.AddQuantity(amountToAdd);
        }
        else
        {
            int amountToAdd = Mathf.Min(item.quantityToPickup, item.stackSize);
            items.Add(new InventorySlot(item, amountToAdd));
        }
        Debug.Log($"Added {item.quantityToPickup} of {item.itemName}.");
    }

    public void ConsumeKey(ItemSO keyItem)
    {
        InventorySlot keySlot = items.Find(slot => slot.item == keyItem);

        if(keySlot != null)
        {
            keySlot.RemoveQuantity(1);
        }
    }

    public bool HasKey(ItemSO keyItem)
    {
        return items.Exists(slot => slot.item == keyItem && slot.quantity > 0);
    }

    public bool HasAllItems(List<ItemSO> requiredItems)
    {
        foreach (ItemSO requiredItem in requiredItems)
        {
            InventorySlot slot = items.Find(s => s.item == requiredItem);


            if (slot == null || slot.quantity < requiredItem.quantityToPickup)
            {
                return false;
            }
        }
        return true;
    }

    public int GetMutagenCount()
    {
        InventorySlot mutagenSlot = items.Find(s => s.item.itemName == "Mutagen Sample");
        return mutagenSlot != null ? mutagenSlot.quantity : 0;
    }

    public bool TrySpendMutagens(int amountToSpend)
    {
        InventorySlot mutagenSlot = items.Find(s => s.item.itemName == "Mutagen Sample");
        if (mutagenSlot != null && mutagenSlot.quantity >= amountToSpend)
        {
            mutagenSlot.RemoveQuantity(amountToSpend);
            UpdateMutagenDisplay();
            return true;
        }
        return false;
    }

    public void UpdateMutagenDisplay()
    {
        if (mutagenCountText != null)
        {
            mutagenCountText.text = GetMutagenCount().ToString();
        }
    }

    public void AddWeapon(WeaponSO newWeapon)
    {

        if (newWeapon == null)
        {
            return;
        }

        if (!weaponInventory.Contains(newWeapon))
        {
            weaponInventory.Add(newWeapon);
            weaponListPos = weaponInventory.Count - 1;
            EquipWeapon();
            Debug.Log("Picked up: " + newWeapon.name);
        }
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        return weaponInventory.Contains(weapon);
    }

    public void EquipWeapon()
    {
        if (weaponInventory.Count == 0 || weaponInventory[weaponListPos] == null)
        {
            return;
        }

        if (currentWeaponScript != null)
        {
            equippedWeapon.currentAmmoInMag = currentWeaponScript.GetAmmoInMag();
            equippedWeapon.currentAmmoInReserve = currentWeaponScript.GetAmmoInReserve();
            equippedWeapon.savedMode = currentWeaponScript.currentFireMode;

            Destroy(currentWeaponInstance);
        }

        equippedWeapon = weaponInventory[weaponListPos];

        currentWeaponInstance = Instantiate(equippedWeapon.weaponModel, weaponSocket.transform);
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;

        currentWeaponScript = currentWeaponInstance.GetComponent<Weapon>();
        if (currentWeaponScript != null)
        {
            currentWeaponScript.InitializeWeapon(equippedWeapon, refillMag: false);
            currentWeaponScript.SetAmmoState(equippedWeapon.currentAmmoInMag, equippedWeapon.currentAmmoInReserve);
            currentWeaponScript.muzzleFlash = playerRef.playerMuzzleFlash;
            currentWeaponScript.currentFireMode = equippedWeapon.savedMode;

        }
    }

    public void SwitchWeapon(int direction)
    {
        if (weaponInventory.Count == 0 || gamemanager.instance.isPaused)
        {
            return;
        }

        weaponListPos += direction;

        if (weaponListPos < 0)
        {
            weaponListPos = weaponInventory.Count - 1;

        } 
        else if (weaponListPos > weaponInventory.Count - 1)
        {
            weaponListPos = 0;
        }
            //weaponListPos = Mathf.Clamp(weaponListPos, 0, weaponInventory.Count - 1);
            EquipWeapon();

        
    }

    public int GetAmmoAmount(string ammoName)
    {
        InventorySlot ammoSlot = items.Find(s => s.item.itemName == ammoName);
        return ammoSlot != null ? ammoSlot.quantity : 0;
    }

    public bool TryGetAmmoAmount(AmmoType type, out int amount)
    {
        amount = 0;
        if (ammoLookup.TryGetValue(type, out string ammoName))
        {
            amount = GetAmmoAmount(ammoName);
            return true;
        }
        return false;
    }

    public void ConsumeAmmo(int amount)
    {
        foreach (ItemSO item in collectedItems)
        {
            if (item.itemName == "Ammo")
            {
                item.quantityHeld -= amount;
                item.quantityHeld = Mathf.Max(0, item.quantityHeld);
                break;
            }

            
        }
    }

    public void ConsumeAmmoByType(AmmoType type, int amount)
    {
        if (ammoLookup.TryGetValue(type, out string ammoName))
        {
            InventorySlot ammoSlot = items.Find(s => s.item.itemName == ammoName);
            if (ammoSlot != null)
            {
                ammoSlot.RemoveQuantity(amount);
            }
        }
    }

    public string GetAmmoNameByType(AmmoType type)
    {
        if (ammoLookup.TryGetValue(type, out string ammoName))
            return ammoName;

        Debug.LogWarning("AmmoType not found in lookup: " + type);
        return string.Empty;
    }

    public Weapon GetActiveWeapon()
    {
        return currentWeaponScript;
    }
}
