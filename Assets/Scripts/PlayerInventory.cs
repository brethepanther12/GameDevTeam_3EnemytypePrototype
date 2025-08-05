using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    [HideInInspector] public playerController playerRef;

    [Header("Weapon Inventory")]
    public List<ItemSO> collectedItems = new List<ItemSO>();
    public List<WeaponSO> weaponInventory = new List<WeaponSO>();
    public WeaponSO equippedWeapon;
    public int weaponListPos = 0;

    [Header("Runtime References")]
    public GameObject weaponSocket;
    private GameObject currentWeaponInstance;
    private Weapon currentWeaponScript;


    private Dictionary<AmmoType, string> ammoLookup = new Dictionary<AmmoType, string>
    {

    { AmmoType.AR, "Rifle Ammo" },
    { AmmoType.Shell, "Shotgun Shells" },
    { AmmoType.Grenade, "Frag Round" }



    };



    public void AddItem(ItemSO item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            item.quantityHeld = item.quantityToPickup;
            Debug.Log("Picked up: " + item.itemName);
        }
        else
        {
            ItemSO existingItem = collectedItems.Find(x => x == item);

            if (existingItem != null && existingItem.quantityHeld < existingItem.stackSize)
            {
                int availableSpace = existingItem.stackSize - existingItem.quantityHeld;
                int amountToAdd = Mathf.Min(item.quantityToPickup, availableSpace);
                existingItem.quantityHeld += amountToAdd;
            }
        }
    }

    public bool HasAllItems(List<ItemSO> requiredItems)
    {
        foreach (ItemSO requiredItem in requiredItems)
        {
            ItemSO ownedItem = collectedItems.Find(item => item == requiredItem);

            if (ownedItem == null || ownedItem.quantityHeld < requiredItem.quantityToPickup)
            {
                return false;
            }
        }
        return true;
    }
    public void AddWeapon(WeaponSO newWeapon)
    {
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
        if (weaponInventory.Count == 0)
        {
            return;
        }

        if (currentWeaponScript != null)
        {
            equippedWeapon.currentAmmoInMag = currentWeaponScript.GetAmmoInMag();
            equippedWeapon.currentAmmoInReserve = currentWeaponScript.GetAmmoInReserve();
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

        }
    }

    public void SwitchWeapon(int direction)
    {
        if (weaponInventory.Count == 0)
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
        foreach (ItemSO item in collectedItems)
        {
            if (item.itemName == ammoName)
            {
                return item.quantityHeld;
            }
                
        }
        return 0;
    }

    public bool TryGetAmmoAmount(AmmoType type, out int amount)
    {
        amount = 0;

        if (ammoLookup.TryGetValue(type, out string ammoName))
        {
            foreach (ItemSO item in collectedItems)
            {
                if (item.itemName == ammoName)
                {
                    amount = item.quantityHeld;
                    return true;
                }
            }
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
            foreach (ItemSO item in collectedItems)
            {
                if (item.itemName == ammoName)
                {
                    item.quantityHeld -= amount;
                    item.quantityHeld = Mathf.Max(0, item.quantityHeld);
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("Tried to consume ammo for unknown AmmoType: " + type);
        }
    }

    public string GetAmmoNameByType(AmmoType type)
    {
        if (ammoLookup.TryGetValue(type, out string ammoName))
            return ammoName;

        Debug.LogWarning("AmmoType not found in lookup: " + type);
        return string.Empty;
    }
}
