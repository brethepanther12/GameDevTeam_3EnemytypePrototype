using System.Collections.Generic;

[System.Serializable]
public class WeaponRuntimeData
{
    public int currentAmmoInMag;
    public FireMode savedMode;
    public List<WeaponUpgradeSO> purchasedUpgrades = new List<WeaponUpgradeSO>();

    public WeaponRuntimeData(int startingAmmo)
    {
        currentAmmoInMag = startingAmmo;
        savedMode = FireMode.Semi;
    }
}