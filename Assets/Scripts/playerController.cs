using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class playerController : MonoBehaviour, IDamage, Visibility
{

    [Header("--- Animation ---")]
    public Animator animator;

    [Header("--- Components & Stats ---")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] float speed;
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

    [SerializeField] float dashCooldown;
    [SerializeField] float dashDuration;
    [SerializeField] int dashCount;
    [SerializeField] int maxDashCount;
    private bool isDashing;

    [Header("--- Audio ---")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private float hurtVol;
    [SerializeField] private float footstepVol = 1f;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float sprintStepDelay = 0.3f;

    [Header("--- Game Objects & Systems ---")]
    public ParticleSystem playerMuzzleFlash;
    public PlayerInventory inventory;
    public GameObject weaponSocket;
    public int jumpCur;

    private MovingPlatformStick targetPlatform;

    float stepTimer = 0f;
    public bool isReloading;
    public bool isVisible;
    public int currentAmmo;

    private float originalSpeed;
    private Coroutine slowRoutine;

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
    //float sprintTimer;
    //public float sprintCD;

    void Start()
    {
        originalSpeed = this.speed;
        HPOrig = HP;
        armorOrig = armor;
        shieldOrig = shield;
        dashCount = maxDashCount;
        StartCoroutine(RechargeDash());
        spawnPlayer();

        inventory = GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            inventory.weaponSocket = weaponSocket;
            inventory.playerRef = this;

            foreach (var weapon in GlobalInventory.instance.collectedWeapons)
            {
                inventory.AddWeapon(weapon);
            }
        }
    }

    void Update()
    {
        //sprint();
        movement();
        HandleWeaponSwitching();

        if (gamemanager.instance.isPaused)
            return;

        if (Input.GetButtonDown("Sprint") && dashCount > 0 && !isDashing)
        {
            StartCoroutine(Dash());
            StartCoroutine(UpdateDashCooldown());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (animator != null)
            {
                animator.SetTrigger("Reload");
            }

            Weapon currentWeapon = inventory.GetActiveWeapon();

            if (currentWeapon != null)
            {
                currentWeapon.StartReload();
            }
        }
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
            animator.SetBool("isGrounded", IsGrounded());
            playerVel = Vector3.zero;
            jumpCount = 0;
            jumpCur = 0;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);

        //Getting platform velocity
        Vector3 movePlatform = Vector3.zero;
        if (targetPlatform != null)
        {
            movePlatform = targetPlatform.GetPlatformVelocity();
        }

        controller.Move(moveDir * speed * Time.deltaTime + movePlatform * Time.deltaTime);

        float horizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", horizontalSpeed);
        }

        HandleFootsteps();
        jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (controller != null)
        {
            if (hit.collider.CompareTag("Moving Platform"))
                targetPlatform = hit.collider.GetComponent<MovingPlatformStick>();
            else
                targetPlatform = null;
        }
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
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }

            if (playerVel.y < jumpVel)
            {
                playerVel.y = jumpVel;
            }

            jumpCount++;
            jumpCur = jumpCount;
            updatePlayerUI();
        }
    }

    //void sprint()
    //{
    //    if (Input.GetButtonDown("Sprint"))
    //    {

    //     speed *= sprintMod;

    //    }
    //    else if (Input.GetButtonUp("Sprint"))
    //    {
    //        speed /= sprintMod;

    //    }
    //}

    public void takeDamage(int amount)
    {
        if (shield > 0)
        {
            shield -= amount;

            if (shield < 0)
            {
                shield = 0;
            }
            updatePlayerUI();
            StartCoroutine(ShieldDamageFlashScreen());
        }
        else if (armor > 0)
        {
            armor -= amount;

            if (armor < 0)
            {
                armor = 0;
            }
            updatePlayerUI();
            StartCoroutine(ArmorDamageFlashScreen());
        }
        else
        {
            HP -= amount;

            if (HP < 0)
            {
                HP = 0;
            }
            AudioSource.PlayClipAtPoint(hurtSound, transform.position, hurtVol);
            updatePlayerUI();
            StartCoroutine(damageFlashScreen());
        }

        if (HP <= 0)
        {
            gamemanager.instance.youLose();
        }
    }

    public void takeDamage(int amount, StatusEffectData effect)
    {
        switch (effect.statusType)
        {

            case DamageStatus.None:

                takeDamage(amount);
                break;

            case DamageStatus.Fire:

                if (shield <= 0 && armor <= 0 && HP > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Corrosive:

                if (shield <= 0 && armor > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Cryo:

                takeDamage(amount);
                break;

            case DamageStatus.Electric:

                if (shield > 0)
                {
                    takeDamage(amount + 1);
                }
                else
                {
                    takeDamage(amount);
                }
                break;

            case DamageStatus.Explosive:

                takeDamage(amount);
                break;

            default:
                break;
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

    public void IncreaseDamage(int amount, int magnitude)
    {
        if (magnitude >= 1)
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
        if (magnitude >= 1)
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
        if (magnitude >= 1)
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
        gamemanager.instance.dashCounter.text = $"{dashCount.ToString()} / {maxDashCount.ToString()}";
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
            int mag = activeWep.GetAmmoInMag();
            int reserve = 0;
            inventory.TryGetAmmoAmount(activeWep.ammoType, out reserve);

            gamemanager.instance.ammoText.text = $"{mag} / {reserve}";
            gamemanager.instance.fireModeText.text = $"{activeWep.currentFireMode}";
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

    public void SetInvisible(bool state)
    {
        isVisible = state;
    }

    public bool IsInvisible()
    {
        return isVisible;
    }

    public Vector3 GetVerticalVelocity()
    {
        return playerVel;
    }

    public void SetVerticalVelocity(Vector3 velocity)
    {
        playerVel = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Smoke"))
            isVisible = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Smoke"))
            isVisible = false;
    }

    public void slowDown(float magnitude, float duration)
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(SlowRoutine(magnitude, duration));
    }

    private IEnumerator SlowRoutine(float magnitude, float duration)
    {

        if (this == null) yield break;

        if (originalSpeed == 0f)
            originalSpeed = speed;

        float slowedSpeed = originalSpeed * (1f - magnitude);
        speed = slowedSpeed;

        yield return new WaitForSeconds(duration);

        speed = originalSpeed;
        slowRoutine = null;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashCount--;

        speed = originalSpeed * sprintMod;

        yield return new WaitForSeconds(dashDuration);

        speed = originalSpeed;
        isDashing = false;
    }

    private IEnumerator RechargeDash()
    {
        while (true)
        {
            if (dashCount < maxDashCount)
            {
                yield return new WaitForSeconds(dashCooldown);
                dashCount++;
            }
            else
            {
                yield return null;
            }
        }
    }

    public IEnumerator UpdateDashCooldown()
    {

        float timeElapsed = 0f;

        while (timeElapsed < dashCooldown)
        {
            timeElapsed += Time.deltaTime;

            float fill = Mathf.Clamp01(timeElapsed / dashCooldown);
            gamemanager.instance.dashCounterCDImage.fillAmount = fill;

            yield return null;
        }

        gamemanager.instance.dashCounterCDImage.fillAmount = 1f;
    }

    public int GetBaseShootDamage()
    {
        return shootDamage;
    }
}