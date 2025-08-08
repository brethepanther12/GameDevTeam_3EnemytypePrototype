using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public Camera playerCamera;

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
    public FireMode currentFireMode;
    public int fireModeIndex;

    //Info for shooting
    public Transform leftHandGrip;
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
    bool isBursting;
    int ammoInMag;
    int ammoInReserve;
    public Animator gunAnim;
    public ParticleSystem muzzleFlash;


    private void Awake()
    {
        equippedPlayer = gamemanager.instance?.playerScript;
        inventory = equippedPlayer?.GetComponent<PlayerInventory>();

        if (playerCamera == null)
            playerCamera = Camera.main;
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
        currentFireMode = weaponData.availableFireModes[fireModeIndex];

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

        if (currentFireMode == FireMode.Semi)
        {

            if (Input.GetButtonDown("Fire1") && shootTimer >= attackRate && ammoInMag > 0)
            {
                shootTimer = 0f;
                if (ammoType == AmmoType.AR || ammoType == AmmoType.Grenade || ammoType == AmmoType.Rocket)
                    Shoot();
                else if (ammoType == AmmoType.Shell)
                    ShootMultiple();

            }
        } 
        else if (currentFireMode == FireMode.Auto)
        {
            if (Input.GetButton("Fire1") && shootTimer >= attackRate && ammoInMag > 0)
            {
                shootTimer = 0f;
                if (ammoType == AmmoType.AR || ammoType == AmmoType.Grenade || ammoType == AmmoType.Rocket)
                    Shoot();
                else if (ammoType == AmmoType.Shell)
                    ShootMultiple();

            }

        }
        else if (currentFireMode == FireMode.Burst)
        {

            if (Input.GetButtonDown("Fire1") && shootTimer >= attackRate && ammoInMag > 0)
            {
                StartCoroutine(BurstFire());

            }

        }



        if (Input.GetKeyDown(KeyCode.R) && ammoInMag < magSize && !equippedPlayer.isReloading)
        {
            // This is now handled by the playerController, but we can leave it here
            // as a backup or for different behavior if needed.
            // StartCoroutine(Reload());
        }

        if (Input.GetButtonDown("FireModeSelector"))
        {
            if (fireModeIndex < weaponData.availableFireModes.Count - 1)
            {
                fireModeIndex++;
                currentFireMode = weaponData.availableFireModes[fireModeIndex];
            } else
            {
                fireModeIndex = 0;
                currentFireMode = weaponData.availableFireModes[fireModeIndex];
            }

            equippedPlayer.updatePlayerUI();
        }
    }

    private IEnumerator BurstFire()
    {
        isBursting = true;

        shootTimer = 0f;

        int shotsToFire = Mathf.Min(3, ammoInMag);

        for (int i = 0; i < shotsToFire; i++)
        {
            if (ammoType == AmmoType.Shell)
                ShootMultiple();
            else
                Shoot();

            yield return new WaitForSeconds(0.1f);

            if (ammoInMag <= 0)
                break;
        }

        isBursting = false;
    }

    void Shoot()
    {
        
        ammoInMag--;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunAudio != null && gunShotSound != null)
            gunAudio.PlayOneShot(gunShotSound);

        // --- Camera-Aiming Logic ---
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, range))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }
        // --- End of Camera-Aiming Logic ---

        if (equippedPlayer != null && equippedPlayer.animator != null)
        {
            equippedPlayer.animator.SetTrigger("Shoot");
        }

        Vector3 direction = (targetPoint - shootPos.position).normalized;

        GameObject bulletObj = Instantiate(bullet, shootPos.position, Quaternion.LookRotation(direction));
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

        // --- Camera-Aiming Logic (Done once for all pellets) ---
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, range))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(range);
        }
        // --- End of Camera-Aiming Logic ---

        if (equippedPlayer != null && equippedPlayer.animator != null)
        {
            equippedPlayer.animator.SetTrigger("Shoot");
        }

        equippedPlayer.updatePlayerUI();

        for (int i = 0; i < pellets; i++)
        {
            // Calculate a random direction with spread
            Vector3 direction = (targetPoint - shootPos.position);
            Vector3 spreadVector = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread)
            );
            direction = (direction + spreadVector).normalized;


            GameObject pellet = Instantiate(bullet, shootPos.position, Quaternion.LookRotation(direction));
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

        yield return new WaitForSeconds(1f);

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
        }
        GameObject reticle = GameObject.Find("Reticle");

        if (reticle != null)
        {
            ReticleController rc = reticle.GetComponent<ReticleController>();

            if (rc != null)
            {
                rc.SetEnemyAim(aimingAtEnemy);
            }
        }
    }

    public void StartReload()
    {
        if (ammoInMag < magSize)
            StartCoroutine(Reload());
    }
}