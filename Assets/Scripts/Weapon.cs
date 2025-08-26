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
    public int pellets;
    public float spread;
    public float blastRadius;
    public float energyRecharge;

    public AmmoType ammoType;
    public FireMode currentFireMode;
    public int fireModeIndex;
    public FireModeData FMData;


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
    
    private bool isCharging;
    private float chargeTimer;
    public GameObject chargeEffectPrefab;
    private GameObject activeChargeEffect;
    public AudioClip chargeSound;
    public AudioClip chargeFinished;
    private bool hasPlayedChargeCompleteSound;
    private Grenade activeGrenade;


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
      //  Debug.LogWarning("--- WEAPON INITIALIZED: " + data.weaponName + " at time " + Time.time + " ---");

        weaponData = data;

        currentFireMode = weaponData.savedMode;
        FMData = weaponData.GetFireModeData(currentFireMode);
        ApplyFireModeStats();

        impactSound = weaponData.impactSound;
        impactVolume = weaponData.impactVolume;
        reloadSound = weaponData.reloadSound;
        gunShotSound = weaponData.gunShotSound;

        if (refillMag)
            ammoInMag = magSize;

        shootTimer = 0f;

        if (FMData.projectileType == AmmoType.Energy && ammoInMag != magSize)
        {
            StartCoroutine(RechargeEnergy());
        }
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


        if (gamemanager.instance.isPaused)
            return;


        if (currentFireMode == FireMode.Semi)
        {
            if (Input.GetButtonDown("Fire1") && shootTimer >= attackRate && ammoInMag > 0 && !equippedPlayer.isReloading)
            {
                shootTimer = 0f;
                StartCoroutine(FireWhenReady());
            }
        }
        else if (currentFireMode == FireMode.Auto)
        {
            if (Input.GetButton("Fire1") && shootTimer >= attackRate && ammoInMag > 0 && !equippedPlayer.isReloading)
            {
                shootTimer = 0f;
                StartCoroutine(FireWhenReady());
            }
        }
        else if (currentFireMode == FireMode.Burst)
        {
            if (Input.GetButtonDown("Fire1") && shootTimer >= attackRate && ammoInMag > 0 && !isBursting && !equippedPlayer.isReloading)
            {
                shootTimer = 0f;
                StartCoroutine(FireWhenReady());
            }
        }
        else if (currentFireMode == FireMode.Charge)
        {

            if (Input.GetButtonDown("Fire1") && ammoInMag > 0)
            {
                isCharging = true;
                chargeTimer = 0f;
                hasPlayedChargeCompleteSound = false;

                if (chargeEffectPrefab != null && shootPos != null)
                {
                    activeChargeEffect = Instantiate(chargeEffectPrefab, shootPos.position, shootPos.rotation, shootPos);

                    if (chargeSound != null && gunAudio != null)
                    {
                        gunAudio.loop = true;
                        gunAudio.clip = chargeSound;
                        gunAudio.Play();

                    }
                    
                }
            }

            if (Input.GetButton("Fire1") && isCharging)
            {
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= FMData.chargeTime && !hasPlayedChargeCompleteSound)
                {
                    hasPlayedChargeCompleteSound = true;

                    if (chargeFinished != null)
                    {
                        gunAudio.Stop();
                        gunAudio.PlayOneShot(chargeFinished);
                    }
                }
            }

            if (Input.GetButtonUp("Fire1") && isCharging)
            {
                isCharging = false;

                if (gunAudio != null && gunAudio.isPlaying)
                {
                    gunAudio.Stop();
                }

                if (activeChargeEffect != null)
                {
                    Destroy(activeChargeEffect);
                    activeChargeEffect = null;
                }
                if (chargeTimer >= FMData.chargeTime)
                {
                    if (ammoType == AmmoType.Shell)
                        ShootMultiple();
                    else
                        Shoot();
                }

                chargeTimer = 0f;
            }

        }

        else if (currentFireMode == FireMode.Detonate)
        {
            if (Input.GetButtonDown("Fire1") && ammoInMag > 0 && !equippedPlayer.isReloading)
            {
                StartCoroutine(FireWhenReady());
            }

            if (Input.GetButtonUp("Fire1") && activeGrenade != null)
            {
                DetonateGrenade();
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
                
            } else
            {
                fireModeIndex = 0;
                
            }

            currentFireMode = weaponData.availableFireModes[fireModeIndex];
            
            equippedPlayer.updatePlayerUI();
            ApplyFireModeStats();
        }
    }

    private void LaunchDetonateGrenade()
    {
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

        Vector3 direction = (targetPoint - shootPos.position).normalized;

        GameObject grenadeObj = Instantiate(FMData.projectile, shootPos.position, Quaternion.LookRotation(direction));

        activeGrenade = grenadeObj.GetComponent<Grenade>();

        ammoInMag--;
        equippedPlayer.updatePlayerUI();
    }

    private void DetonateGrenade()
    {
        if (activeGrenade != null)
        {
            activeGrenade.RemoteDetonate();
            
            activeGrenade = null;
        }
    }

    public void ApplyFireModeStats()
    {

        FMData = weaponData.GetFireModeData(currentFireMode);
        
        if (equippedPlayer != null)
        {
            wepDmg = FMData.damage + equippedPlayer.GetBaseShootDamage();
        } else
        {
            wepDmg = FMData.damage;
           // Debug.Log($"<color=orange>STATS RESET:</color> ApplyFireModeStats ran. wepDmg reset to {wepDmg}");
        }
            
        attackRate = FMData.fireRate;
        range = FMData.range;
        pellets = FMData.projectileCount;
        spread = FMData.projectileSpread;
        ammoType = FMData.projectileType;
        bullet = FMData.projectile;
        blastRadius = FMData.blastRadius;
        energyRecharge = FMData.energyRechargeRate;
    }

    private IEnumerator BurstFire()
    {
        isBursting = true;

        shootTimer = 0f;

        int bc = FMData.burstCount;
        float br = FMData.burstRate;

        int shotsToFire = Mathf.Min(bc, ammoInMag);

        for (int i = 0; i < shotsToFire; i++)
        {
            if (ammoType == AmmoType.Shell)
                ShootMultiple();
            else
                Shoot();

            yield return new WaitForSeconds(br);

            if (ammoInMag <= 0)
                break;
        }

        isBursting = false;
    }

    void Shoot()
    {
      //  Debug.Log($"<color=cyan>FIRING:</color> Bullet is using {wepDmg} damage.");

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
        {
            if (FMData.effectData != null && FMData.effectData.statusType != DamageStatus.None)
            {
                dmgScript.SetStatusData(FMData.effectData);
            }
            dmgScript.SetWeaponDamage(wepDmg);
            dmgScript.SetBlastRadius(FMData.blastRadius);
        }
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
      //  Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * range, Color.red);
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

    private IEnumerator RechargeEnergy()
    {
        while (true)
        {
            int magSizeOrig = magSize;

            if (ammoInMag < magSizeOrig)
            {
                yield return new WaitForSeconds(energyRecharge);

                ammoInMag++;
            }
            else
            {
                yield return null;
            }
        }
    }


    public void ApplyUpgrade(WeaponUpgradeSO upgrade)
    {
        if (upgrade.isFireModeUnlock)
        {
            if (!weaponData.availableFireModes.Contains(upgrade.fireModeToUnlock))
            {
                weaponData.availableFireModes.Add(upgrade.fireModeToUnlock);
            }
            return;
        }

        switch (upgrade.statToUpgrade)
        {
            case WeaponStatType.Damage:
                wepDmg += (int)upgrade.upgradeAmount;
               // Debug.Log($"<color=green>UPGRADE APPLIED:</color> wepDmg is now {wepDmg}");
                break;
            case WeaponStatType.MagSize:
                magSize += (int)upgrade.upgradeAmount;
                break;
            case WeaponStatType.AttackRate:
                attackRate -= upgrade.upgradeAmount;
                if (attackRate < 0.05f) attackRate = 0.05f;
                break;
            case WeaponStatType.Range:
                range += (int)upgrade.upgradeAmount;
                break;
            case WeaponStatType.MaxAmmo:
                inventory.ApplyMaxAmmoUpgrade(this.ammoType, (int)upgrade.upgradeAmount);
                break;
        }
    }

    private IEnumerator FireWhenReady()
    {
        //gunAnim.SetTrigger("Shoot");

        yield return null;

        while (gunAnim.IsInTransition(0))
        {
            yield return null;
        }

        if (currentFireMode == FireMode.Burst)
        {
            StartCoroutine(BurstFire());
        }
        else if(currentFireMode == FireMode.Detonate)
        {
            LaunchDetonateGrenade();
        }
        else
        {
            if (ammoType == AmmoType.Shell)
                ShootMultiple();
            else
                Shoot();
        }
    }

    public bool CanReload()
    {
        if (ammoInMag < magSize && equippedPlayer.isReloading == false && ammoInReserve > 0)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public bool IsEmpty()
    {
        if (ammoInMag == 0)
        {
            return true;
        } else
        {
            return false;
        }
    }
}