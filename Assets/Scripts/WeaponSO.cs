using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponSO : ScriptableObject
{

    public LayerMask ignoreLayer;

    public string weaponName;

    //Weapon Stats
    public int wepDmg;
    public float attackRate;
    public int range;
    public int magSize;
    public int ammoMax;
    
    public GameObject bullet;
    public GameObject impactPrefab;
    public GameObject weaponModel;

    //Gun Audio
    public AudioClip impactSound;
    public float impactVolume = 1f;
    public AudioClip reloadSound;
    public AudioClip gunShotSound;

}
