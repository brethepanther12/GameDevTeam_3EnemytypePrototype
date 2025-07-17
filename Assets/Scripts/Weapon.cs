using UnityEngine;
using System.Collections;
public class Weapon : MonoBehaviour { 

    [SerializeField] int wepDmg; 
    [SerializeField] float attackRate; 
    [SerializeField] int range; 
    [SerializeField] int magSize; 
    //[SerializeField] int ammoReserve; 
    [SerializeField] int ammoMax; 
    [SerializeField] LayerMask ignoreLayer; 
    [SerializeField] GameObject bullet; 
    [SerializeField] Transform shootPos; 
    [SerializeField] private AudioClip impactSound; 
    [SerializeField] private float impactVolume = 1f; 
    [SerializeField] private AudioClip reloadSound; 
    [SerializeField] private AudioSource gunAudio; 
    [SerializeField] private AudioClip gunShotSound; 

    playerController equippedPlayer; 
    PlayerInventory inventory; 

    float shootTimer; 
    int ammoInMag; 
    int ammoInReserve; 
    public Animator gunAnim; 
    public ParticleSystem muzzleFlash; 

    public enum wepInfo 
    { model, image, gunshot_sound, muzzle_flash, impact_sound, impact_volume, reload_sound, gun_audio, animator, impact_prefab } 
    public enum wepStats 
    { bullet_dmg, fireRate, range, magSize, reserveSize } 

    private void Start() 
    { 
        equippedPlayer = gamemanager.instance.playerScript; 
        inventory = equippedPlayer.GetComponent<PlayerInventory>(); 
        ammoInMag = magSize;
        ammoInReserve = inventory.GetAmmoAmount("Ammo");
        equippedPlayer.updatePlayerUI();
    } 
    private void Update() 
    { 
        shootTimer += Time.deltaTime; 
        CheckReticleTarget(); 

        if (Input.GetButton("Fire1") && shootTimer > attackRate) 
        { 
            Shoot(); 
        } 

        if (Input.GetKeyDown(KeyCode.R) || ammoInMag == 0 && ammoInReserve > 0 && !equippedPlayer.isReloading) 

        { 
            StartCoroutine(Reload());
            
        } 
    } 
    public void Shoot() 
    { 
        shootTimer = 0; 

        if (ammoInMag > 0 && !equippedPlayer.isReloading) 
        { 
            ammoInMag--; 
            muzzleFlash.Play(); 
            gunAudio.pitch = Random.Range(0.95f, 1.05f); 
            gunAudio.PlayOneShot(gunShotSound); 

            bool isEnemy = false; 

            GameObject reticle = GameObject.Find("Reticle"); 

            if (reticle != null) 
            { 
                ReticleController rc = reticle.GetComponent<ReticleController>(); 
                if (rc != null)
                    
                { 
                    rc.Pulse(isEnemy); 
                } 
            } 

            Instantiate(bullet, shootPos.position, transform.rotation); 

            damage dmgScript = bullet.GetComponent<damage>();
            
            if (dmgScript != null) 
            { 
                dmgScript.SetWeaponDamage(wepDmg); 
            } 

            equippedPlayer.updatePlayerUI(); 
        } 
    } 
    IEnumerator Reload() 
    { 
        equippedPlayer.isReloading = true; 
        gunAudio.PlayOneShot(reloadSound); 
        gunAnim.SetBool("Reloading", true);
        
        yield return new WaitForSeconds(1f - .25f); 

        gunAnim.SetBool("Reloading", false); 

        yield return new WaitForSeconds(.25f); 

        int ammoNeeded = magSize - ammoInMag; 
        int ammoToLoad = Mathf.Min(ammoNeeded, inventory.GetAmmoAmount("Ammo")); 

        ammoInMag += ammoToLoad; 
        ammoInReserve -= ammoToLoad;
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