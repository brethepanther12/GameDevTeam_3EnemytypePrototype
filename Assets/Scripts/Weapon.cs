using UnityEngine;
using System.Collections;
public class Weapon : MonoBehaviour {

    public WeaponSO weaponData;
    public LayerMask ignoreLayer;

    //Weapon Stats
    public int wepDmg;
    public float attackRate;
    public int range;
    public int magSize;
    public int ammoMax;

    //Info for shooting
    public GameObject bullet;
    public Transform shootPos;

    //Gun Audio
    public AudioClip impactSound;
    public float impactVolume = 1f;
    public AudioClip reloadSound;
    public AudioSource gunAudio;
    public AudioClip gunShotSound;

    playerController equippedPlayer; 
    PlayerInventory inventory; 

    float shootTimer; 
    int ammoInMag; 
    int ammoInReserve; 
    public Animator gunAnim; 
    public ParticleSystem muzzleFlash;


    private void Awake()
    {
        equippedPlayer = gamemanager.instance?.playerScript;
        inventory = equippedPlayer?.GetComponent<PlayerInventory>();
    }
    private void Start() 
    {
        if(weaponData !=null)
        InitializeWeapon(weaponData);
    }

    public void InitializeWeapon(WeaponSO data, bool refillMag = false)
    {
        weaponData = data;
        wepDmg = weaponData.wepDmg;
        attackRate = weaponData.attackRate;
        range = weaponData.range;
        magSize = weaponData.magSize;
        ammoMax = weaponData.ammoMax;

        bullet = weaponData.bullet;
        impactSound = weaponData.impactSound;
        impactVolume = weaponData.impactVolume;
        reloadSound = weaponData.reloadSound;
        gunShotSound = weaponData.gunShotSound;

        if (refillMag)
            ammoInMag = magSize;

        shootTimer = 0f;
    }

    public void SetAmmoState(int mag, int reserve)
    {
        ammoInMag = mag;
        ammoInReserve = reserve;
    }
    private void Update()
    {
        shootTimer += Time.deltaTime;

        CheckReticleTarget();

        if (Input.GetButton("Fire1") && shootTimer >= attackRate && ammoInMag > 0)
        {
           
            Shoot();
                
        }

        if (Input.GetKeyDown(KeyCode.R) && ammoInMag < magSize && !equippedPlayer.isReloading)
        {
            StartCoroutine(Reload());
        }
    }
    void Shoot()
    {
        shootTimer = 0f;
        ammoInMag--;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunAudio != null && gunShotSound != null)
            gunAudio.PlayOneShot(gunShotSound);

        GameObject bulletObj = Instantiate(bullet, shootPos.position, shootPos.rotation);
        damage dmgScript = bulletObj.GetComponent<damage>();
        if (dmgScript != null)
            dmgScript.SetWeaponDamage(wepDmg);

        equippedPlayer.updatePlayerUI();
    }
    IEnumerator Reload()
    {
        equippedPlayer.isReloading = true;

        if (reloadSound != null)
            gunAudio.PlayOneShot(reloadSound);

        if (gunAnim != null)
            gunAnim.SetBool("Reloading", true);

        yield return new WaitForSeconds(1f);

        if (gunAnim != null)
            gunAnim.SetBool("Reloading", false);

        int ammoNeeded = magSize - ammoInMag;
        int ammoToLoad = Mathf.Min(ammoNeeded, inventory.GetAmmoAmount("Ammo"));

        ammoInMag += ammoToLoad;
        inventory.ConsumeAmmo(ammoToLoad);
        ammoInReserve = inventory.GetAmmoAmount("Ammo");

        equippedPlayer.isReloading = false;
        equippedPlayer.updatePlayerUI();
    }
    public int GetAmmoInMag()
    {
        return ammoInMag;
    }
    public int GetAmmoInReserve()
    {
        return ammoInReserve;
    }
    void CheckReticleTarget() 
    { 
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * range, Color.red); 
        RaycastHit hit; 
        bool aimingAtEnemy = false; 

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, ~ignoreLayer)) 
        { 
            if (hit.collider.CompareTag("Enemy")) 
            { 
                aimingAtEnemy = true; 
            } 
        } GameObject reticle = GameObject.Find("Reticle"); 

        if (reticle != null) 
        { 
            ReticleController rc = reticle.GetComponent<ReticleController>(); 

            if (rc != null) 
            { 
                rc.SetEnemyAim(aimingAtEnemy); 
            } 
        } 
    }
}