using System.Collections.Generic;
using UnityEngine;

public class GlobalInventory : MonoBehaviour
{
    public static GlobalInventory instance;

    public List<WeaponSO> collectedWeapons = new List<WeaponSO>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasWeapon(WeaponSO weapon)
    {
        return collectedWeapons.Contains(weapon);
    }

    public void AddWeapon(WeaponSO weapon)
    {
        if (!HasWeapon(weapon))
        {
            collectedWeapons.Add(weapon);
        }
    }
}