using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Upgrade", menuName = "Upgrades/Ability Upgrade")]
public class AbilityUpgradeSO : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Cost")]
    public int mutagenCost;

    public ItemSO requiredItem;

    [Header("Effect")]
    // use enum i created for upgrades
    public UpgradeType upgradeType;
    public int quantity;
    public int magnitude;
    public bool increaseMax;
}