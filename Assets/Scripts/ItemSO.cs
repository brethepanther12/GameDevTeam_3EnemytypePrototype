using UnityEngine;


public enum ItemType
{
    Weapon, Consumable, Key, Upgrade, Collectable, Ammo
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int quantityToPickup;
    public int quantityHeld;
    public int stackSize;
    public bool isUnique;

    

}