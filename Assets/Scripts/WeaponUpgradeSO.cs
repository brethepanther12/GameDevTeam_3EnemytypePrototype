using UnityEngine;

public enum WeaponStatType { Damage, MagSize, AttackRate, Range, ReloadSpeed }

[CreateAssetMenu(menuName = "Upgrades/Weapon Upgrade")]
public class WeaponUpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea(2, 5)]
    public string description;
    public int componentCost;

    [Header("Upgrade Effect")]
    public bool isFireModeUnlock;

    public FireMode fireModeToUnlock;

    public WeaponStatType statToUpgrade;
    public float upgradeAmount;
}