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
    public int pellets;
    public float spread;

    public AmmoType ammoType;

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

        pellets = weaponData.pelletCount;
        spread = weaponData.pelletSpread;

        ammoType = weaponData.ammoType;

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
            if (ammoType == AmmoType.AR || ammoType == AmmoType.Grenade)
                Shoot();
            else if (ammoType == AmmoType.Shell)
                ShootMultiple();
            //Shoot();
      
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

    void ShootMultiple()
    {
        shootTimer = 0f;
        ammoInMag--;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunAudio != null && gunShotSound != null)
            gunAudio.PlayOneShot(gunShotSound);

        equippedPlayer.updatePlayerUI();

        for (int i = 0; i < pellets; i++)
        {
           
            Vector3 spreadOffset = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );

            Quaternion pelletRotation = Quaternion.Euler(
                shootPos.rotation.eulerAngles + spreadOffset
            );

            GameObject pellet = Instantiate(bullet, shootPos.position, pelletRotation);

            damage dmgScript = pellet.GetComponent<damage>();
            if (dmgScript != null)
            {
                dmgScript.SetWeaponDamage(wepDmg / pellets);
            }
        }
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

        if (inventory.TryGetAmmoAmount(ammoType, out int ammoInStock))
        {
            int ammoToLoad = Mathf.Min(ammoNeeded, ammoInStock);
            ammoInMag += ammoToLoad;
            inventory.ConsumeAmmoByType(ammoType, ammoToLoad);
            ammoInReserve = ammoInStock - ammoToLoad;
        }

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