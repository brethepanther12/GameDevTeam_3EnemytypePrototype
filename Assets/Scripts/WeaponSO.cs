using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public enum AmmoType { Pistol, AR, Shell, Energy, Fuel, Rocket, Grenade}

public enum FireMode { Semi, Auto, Burst, Charge, Detonate}

[Serializable]
public class FireModeData
{
    public FireMode mode;
    public AmmoType projectileType;

    public int damage;
    //public int range;
    public float fireRate;
    public int burstCount;
    public float burstRate;
    //public float chargeTime;
    //public float detonateTime;
    public int projectileCount;

}

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponSO : ScriptableObject
{
    
    public LayerMask ignoreLayer;

    public string weaponName;
    public AmmoType ammoType;

    public List<FireMode> availableFireModes = new List<FireMode>();
    public List<FireModeData> fireModeDatas = new List<FireModeData>();

    public FireMode savedMode;

    //Weapon Stats
    public int wepDmg;
    public float attackRate;
    public int range;
    public int magSize;
    public int ammoMax;

    public int pelletCount;
    public float pelletSpread;

    [HideInInspector] public int currentAmmoInMag;
    [HideInInspector] public int currentAmmoInReserve;
    
    public GameObject bullet;
    public GameObject impactPrefab;
    public GameObject weaponModel;

    //Gun Audio
    public AudioClip impactSound;
    public float impactVolume = 1f;
    public AudioClip reloadSound;
    public AudioClip gunShotSound;

    public FireModeData GetFireModeData(FireMode fm)
    {
        foreach (FireModeData fireModeData in fireModeDatas)
        {
            if (fireModeData.mode == fm)
            {
                
                return fireModeData;
            }

            
        }

        return fireModeDatas[0];
    }

}
