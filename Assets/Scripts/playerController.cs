using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpVel;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] int magazineSize = 15;
    [SerializeField] int reserveAmmo = 90;
    [SerializeField] int shield;
    [SerializeField] int maxShield;
    [SerializeField] int armor;
    [SerializeField] int maxArmor;
    [SerializeField] int shootDamage;
    [SerializeField] int meleeDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    ////Added for grenade logic, delete if causing any errors
    //[SerializeField] private GameObject grenadePrefab;
    //[SerializeField] private Transform throwPoint;
    //[SerializeField] private float throwingForce;
    //[SerializeField] private Transform grenadeThrowOrigin;

    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private float hurtVol;
    [SerializeField] private float footstepVol = 1f;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float sprintStepDelay = 0.3f;

    public ParticleSystem playerMuzzleFlash;
    public PlayerInventory inventory;
    public GameObject weaponSocket;
    public int jumpCur;

    float stepTimer = 0f;
    public bool isReloading;
    public int currentAmmo;

    private enum powerUpType
    {
        health, shield, armor, ammo, speed, jump, damage
    }

    bool hasKey;
    bool isPoweredUp;
    bool hasAmmo;

    int numKeys;

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;
    int HPOrig;
    int armorOrig;
    int shieldOrig;

    float shootTimer;

    void Start()
    {
        HPOrig = HP;
        armorOrig = armor;
        shieldOrig = shield;
        spawnPlayer();

        inventory = GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            inventory.weaponSocket = weaponSocket;
            inventory.playerRef = this;
        }

    }

    void Update()
    {

        sprint();

        movement();

        HandleWeaponSwitching();

    }

    void Awake()
    {
        if (gamemanager.instance != null)
        {
            gamemanager.instance.player = this.gameObject;
        }
    }

    bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(ray, out _, 1.1f, ~ignoreLayer); 
    }


    void movement()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
            jumpCur = 0;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * speed * Time.deltaTime);

        HandleFootsteps();

        jump();

        //Delete if causing issue
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    throwGrenade();
        //}

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

    }

    void HandleFootsteps()
    {
        float velocity = controller.velocity.magnitude;

        if (velocity > 0.2f && IsGrounded())
        {
            float currentStepDelay = Input.GetKey(KeyCode.LeftShift) ? sprintStepDelay : walkStepDelay;

            stepTimer += Time.deltaTime;
            if (stepTimer >= currentStepDelay)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(clip, footstepVol);
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpVel;
            jumpCount++;
            jumpCur = jumpCount;
            updatePlayerUI();
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    public void takeDamage(int amount)
    {
        if (shield > 0)
        {
            shield -= amount;
            updatePlayerUI();
            StartCoroutine(ShieldDamageFlashScreen());
        }
        else if (armor > 0)
        {
            armor -= amount;
            updatePlayerUI();
            StartCoroutine(ArmorDamageFlashScreen());
        }
        else
        {
            HP -= amount;
            AudioSource.PlayClipAtPoint(hurtSound, transform.position, hurtVol);
            updatePlayerUI();
            StartCoroutine(damageFlashScreen());
        }

        if (HP <= 0)
        {
            gamemanager.instance.youLose();
        }
    }


    public void Heal(int amount, bool doesIncreaseMax)
    {
        HP += amount;

        if (doesIncreaseMax)
        {
            maxHP += amount;

        }
        else if (HP >= maxHP && !doesIncreaseMax)
        {

            HP = maxHP;
        }

        updatePlayerUI();

    }

    public void GainArmor(int amount, bool doesIncreaseMax)
    {
        if (doesIncreaseMax)
        {
            maxArmor += amount;
        }

        armor += amount;

        if (armor > maxArmor)
        {
            armor = maxArmor;
        }

        updatePlayerUI();
    }

    public void GainShield(int amount, bool doesIncreaseMax)
    {
        if (doesIncreaseMax)
        {
            maxShield += amount;
        }

        shield += amount;

        if (shield > maxShield)
        {
            shield = maxShield;
        }

        updatePlayerUI();
    }

    //public void GainAmmo(int amount, bool doesIncreaseMax)
    //{
    //    reserveAmmo += amount;



    //    if (doesIncreaseMax)
    //    {
    //        reserveAmmo += amount;

    //    }
    //    updatePlayerUI();

    //}

    public void IncreaseDamage(int amount, int magnitude)
    {
        if (magnitude == 1)
        {
            shootDamage += amount;

        }
        else if (isPoweredUp)
        {
            shootDamage *= magnitude;

        }

        updatePlayerUI();
    }

    public void IncreaseSpeed(int amount, int magnitude)
    {
        if (magnitude == 1)
        {
            speed += amount;

        }
        else if (isPoweredUp)
        {
            speed *= magnitude;

        }

        updatePlayerUI();
    }

    public void IncreaseJumpMaxCount(int amount, int magnitude)
    {
        if (magnitude == 1)
        {
            jumpMax += amount;

        }
        else if (isPoweredUp)
        {
            jumpMax *= magnitude;

        }

        updatePlayerUI();
    }

    public void AddKey(int amount)
    {
        numKeys += amount;

        if (numKeys < 0)
            numKeys = 0;

        hasKey = numKeys > 0;

        updatePlayerUI();
    }

    public bool HasKey()
    {
        return hasKey;
    }

    IEnumerator PowerUp(float duration)
    {
        isPoweredUp = true;
        yield return new WaitForSeconds(duration);
        isPoweredUp = false;
    }

    void HandleWeaponSwitching()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            inventory.SwitchWeapon(1);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            inventory.SwitchWeapon(-1);

        updatePlayerUI();
    }

    public void updatePlayerUI()
    {    

        gamemanager.instance.playerHPBar.fillAmount = (float)HP / maxHP;
        gamemanager.instance.playerShieldBar.fillAmount = (float)shield / maxShield;
        gamemanager.instance.playerArmorBar.fillAmount = (float)armor / maxArmor;
        gamemanager.instance.jumpCounter.text = $"{jumpCur.ToString()} / {jumpMax.ToString()}";
        gamemanager.instance.playerHp.text = $"{HP} / {maxHP}";
        gamemanager.instance.playerArmor.text = $"{armor} / {maxArmor}";
        gamemanager.instance.playerShield.text = $"{shield} / {maxShield}";
        if (inventory.weaponInventory.Count > 0)
        {
            gamemanager.instance.gunName.text = $"{inventory.weaponInventory[inventory.weaponListPos].weaponName}";
        }
        Weapon activeWep = weaponSocket.GetComponentInChildren<Weapon>();

        if (activeWep != null)
        {
            gamemanager.instance.ammoText.text = $"{activeWep.GetAmmoInMag()} / {inventory.GetAmmoAmount("Ammo")}";
        }
        
    }

    IEnumerator damageFlashScreen()
    {
        gamemanager.instance.playerDamagePanel.SetActive(true);
        yield return new WaitForSeconds(.1f);
        gamemanager.instance.playerDamagePanel.SetActive(false);
    }

    IEnumerator ArmorDamageFlashScreen()
    {
        gamemanager.instance.playerArmorDamagePanel.SetActive(true);
        yield return new WaitForSeconds(.1f);
        gamemanager.instance.playerArmorDamagePanel.SetActive(false);
    }

    IEnumerator ShieldDamageFlashScreen()
    {
        gamemanager.instance.playerShieldDamagePanel.SetActive(true);
        yield return new WaitForSeconds(.1f);
        gamemanager.instance.playerShieldDamagePanel.SetActive(false);
    }

    public void spawnPlayer()
    {
        if (controller != null)
            controller.enabled = false;

        transform.position = gamemanager.instance.PlayerSpawnPOS.transform.position;
        transform.localRotation = gamemanager.instance.PlayerSpawnPOS.transform.localRotation;

        if (controller != null)
            controller.enabled = true;

        HP = HPOrig;
        updatePlayerUI();
    }

    //Delete if causing difficulties
    //public void throwGrenade()
    //{
    //    ItemSO grenade = inventory.collectedItems.Find(item => item.itemName == "Grenade");

    //    if (grenade != null && grenade.quantityHeld > 0 && grenadePrefab != null && grenadeThrowOrigin != null)
    //    {
            
    //        Vector3 throwDirection = grenadeThrowOrigin.forward;

           
    //        GameObject grenadeObj = Instantiate(grenadePrefab, grenadeThrowOrigin.position, Quaternion.identity);
    //        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();

    //        if (rb != null)
    //        {
    //            rb.linearVelocity = throwDirection.normalized * throwingForce;
    //        }

    //        grenade.quantityHeld--;
    //        updatePlayerUI();
    //    }
    //}
}