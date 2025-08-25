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
    public event System.Action<int> OnWeaponComponentsChanged;
    [HideInInspector] public playerController playerRef;

    [Header("Item & Currency Inventory")]
    public List<InventorySlot> items = new List<InventorySlot>();

    [Header("Weapon Inventory")]
    public List<ItemSO> collectedItems = new List<ItemSO>();
    private Dictionary<AmmoType, int> bonusAmmoCapacity = new Dictionary<AmmoType, int>();
    public Dictionary<WeaponSO, WeaponRuntimeData> weaponData = new Dictionary<WeaponSO, WeaponRuntimeData>();
    public WeaponSO equippedWeapon;
    public List<WeaponSO> weaponHolster = new List<WeaponSO>();
    public int weaponListPos = 0;

    [Header("Runtime References")]
    public GameObject weaponSocket;
    private GameObject currentWeaponInstance;
    private Weapon currentWeaponScript;

    [Header("UI References")]
    public TMP_Text mutagenCountText;
    public TMP_Text componentCountText;

    [Header("UI Controllers")]
    public WeaponUIController weaponUIController;

    private Dictionary<AmmoType, string> ammoLookup = new Dictionary<AmmoType, string>
    {

    { AmmoType.Pistol, "Pistol Bullets" },
    { AmmoType.AR, "Rifle Ammo" },
    { AmmoType.Shell, "Shotgun Shells" },
    { AmmoType.Grenade, "Frag Round" },
    { AmmoType.Rocket, "Rocket(Homing)" },
    { AmmoType.Energy, "Plasma Battery" },




    };

    private void Start()
    {
        if (weaponUIController == null)
            weaponUIController = FindAnyObjectByType<WeaponUIController>();

        UpdateComponentDisplay();
        UpdateMutagenDisplay();
    }

    public void RecordUpgradeForWeapon(WeaponSO weaponType, WeaponUpgradeSO upgrade)
    {
        if (weaponData.ContainsKey(weaponType))
        {
            weaponData[weaponType].purchasedUpgrades.Add(upgrade);
        }
    }

    public bool HasPurchasedUpgrade(WeaponSO weaponType, WeaponUpgradeSO upgrade)
    {
        return weaponData.ContainsKey(weaponType) && weaponData[weaponType].purchasedUpgrades.Contains(upgrade);
    }
    public void AddItem(ItemSO item)
    {
        bool isAmmo = ammoLookup.ContainsValue(item.itemName);
        AmmoType ammoType = default;
        if (isAmmo)
        {
            ammoType = ammoLookup.FirstOrDefault(x => x.Value == item.itemName).Key;
        }

        InventorySlot slot = items.Find(s => s.item == item);

        if (slot != null)
        {
            int maxCapacity = isAmmo ? GetMaxAmmoForType(ammoType) : slot.item.stackSize;

            int availableSpace = maxCapacity - slot.quantity;
            if (availableSpace <= 0) return;

            int amountToAdd = Mathf.Min(item.quantityToPickup, availableSpace);
            slot.AddQuantity(amountToAdd);
        }
        else
        {
            int maxCapacity = isAmmo ? GetMaxAmmoForType(ammoType) : item.stackSize;
            int amountToAdd = Mathf.Min(item.quantityToPickup, maxCapacity);
            items.Add(new InventorySlot(item, amountToAdd));
        }
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

    public int GetWeaponComponentCount()
    {
        InventorySlot componentSlot = items.Find(s => s.item.itemName == "Weapon Component");
        return componentSlot != null ? componentSlot.quantity : 0;
    }

    public bool TrySpendWeaponComponents(int amountToSpend)
    {
        InventorySlot componentSlot = items.Find(s => s.item.itemName == "Weapon Component");
        if (componentSlot != null && componentSlot.quantity >= amountToSpend)
        {
            componentSlot.RemoveQuantity(amountToSpend);

            OnWeaponComponentsChanged?.Invoke(componentSlot.quantity);

            return true;
        }
        return false;
    }

    public void UpdateComponentDisplay()
    {
        int count = GetWeaponComponentCount();

        if (componentCountText != null)
        {
            componentCountText.text = count.ToString();
        }

        OnWeaponComponentsChanged?.Invoke(count);
    }

    public void AddWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null || weaponData.ContainsKey(newWeapon))
        {
            return;
        }

        weaponHolster.Add(newWeapon);

        weaponData.Add(newWeapon, new WeaponRuntimeData(newWeapon.magSize));

        weaponListPos = weaponHolster.Count - 1;
        EquipWeapon();
      //  Debug.Log("Picked up: " + newWeapon.name);
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        return weaponHolster.Contains(weapon);
    }

    public void EquipWeapon()
    {
        if (weaponHolster.Count == 0) return;

        if (currentWeaponScript != null)
        {

            if (weaponData.TryGetValue(equippedWeapon, out WeaponRuntimeData oldData))
            {
                oldData.currentAmmoInMag = currentWeaponScript.GetAmmoInMag();
                oldData.savedMode = currentWeaponScript.currentFireMode;
            }
            Destroy(currentWeaponInstance);
        }

        equippedWeapon = weaponHolster[weaponListPos];
        WeaponRuntimeData newData = weaponData[equippedWeapon];

        currentWeaponInstance = Instantiate(equippedWeapon.weaponModel, weaponSocket.transform);
        currentWeaponScript = currentWeaponInstance.GetComponent<Weapon>();

        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;

        if (currentWeaponScript != null)
        {
            currentWeaponScript.InitializeWeapon(equippedWeapon);

            currentWeaponScript.SetAmmoState(newData.currentAmmoInMag, GetAmmoAmount(GetAmmoNameByType(equippedWeapon.ammoType)));
            currentWeaponScript.currentFireMode = newData.savedMode;
            currentWeaponScript.ApplyFireModeStats();

            foreach (WeaponUpgradeSO upgrade in newData.purchasedUpgrades)
            {
                currentWeaponScript.ApplyUpgrade(upgrade);
            }

            currentWeaponScript.muzzleFlash = playerRef.playerMuzzleFlash;
        }

        if (weaponUIController != null)
        {
            weaponUIController.UpdateForNewWeapon(equippedWeapon);
        }
    }

    public void SwitchWeapon(int direction)
    {
        if (weaponHolster.Count == 0 || gamemanager.instance.isPaused)
        {
            return;
        }

        weaponListPos += direction;

        if (weaponListPos < 0)
        {
            weaponListPos = weaponHolster.Count - 1;

        } 
        else if (weaponListPos > weaponHolster.Count - 1)
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

       // Debug.LogWarning("AmmoType not found in lookup: " + type);
        return string.Empty;
    }

    public Weapon GetActiveWeapon()
    {
        return currentWeaponScript;
    }

    public void NotifyWeaponComponentsChanged()
    {
        OnWeaponComponentsChanged?.Invoke(GetWeaponComponentCount());
    }

    public void ApplyMaxAmmoUpgrade(AmmoType ammoType, int amount)
    {
        if (!bonusAmmoCapacity.ContainsKey(ammoType))
        {
            bonusAmmoCapacity[ammoType] = 0;
        }

        bonusAmmoCapacity[ammoType] += amount;
        //Debug.Log($"Max ammo for {ammoType} increased by {amount}. New bonus is {bonusAmmoCapacity[ammoType]}.");
    }

    public int GetMaxAmmoForType(AmmoType ammoType)
    {
        string ammoName = GetAmmoNameByType(ammoType);
        InventorySlot ammoSlot = items.Find(s => s.item.itemName == ammoName);

        int baseStackSize = 100;
        if (ammoSlot != null)
        {
            baseStackSize = ammoSlot.item.stackSize;
        }

        int bonus = 0;
        if (bonusAmmoCapacity.ContainsKey(ammoType))
        {
            bonus = bonusAmmoCapacity[ammoType];
        }

        return baseStackSize + bonus;
    }
}
