using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public enum AmmoType { Pistol, AR, Shell, Energy, Fuel, Rocket, Grenade}
public enum FireMode { Semi, Auto, Burst, Charge, Detonate}


[Serializable]
public class StatusEffectData
{
    public DamageStatus statusType;
    public int statusDamage;
    public float statusTickRate;
    public float statusDuration;
    public float slowDownMagnitude;

}

[Serializable]
public class FireModeData
{
    public FireMode mode;
    public AmmoType projectileType;
    
    public GameObject projectile;
    public StatusEffectData effectData;
    public int damage;
    public int range;
    public float fireRate;
    //public int magSize;
    public int burstCount;
    public float burstRate;
    public float chargeTime;
    public float detonateTime;
    public int projectileCount;
    public float projectileSpread;
    public float blastRadius;
    public float energyRechargeRate;
    
}

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponSO : ScriptableObject
{
    
    public LayerMask ignoreLayer;

    public string weaponName;
    public AmmoType ammoType;

    public List<FireMode> availableFireModes = new List<FireMode>();
    public List<FireModeData> fireModeDatas = new List<FireModeData>();
    public List<WeaponUpgradeSO> availableUpgrades;


    public FireMode savedMode;

    //Weapon Stats
    public int wepDmg;
    public float attackRate;
    public int range;
    public int magSize;

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
