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

    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private float hurtVol;

    [SerializeField] private float footstepVol = 1f;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float walkStepDelay = 0.5f;
    [SerializeField] private float sprintStepDelay = 0.3f;

    [SerializeField] Weapon weapon1;

    public Weapon equippedWeapon;
    public PlayerInventory inventory;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        equippedWeapon = weapon1;

        inventory = GetComponent<PlayerInventory>();

        HPOrig = HP;
        armorOrig = armor;
        shieldOrig = shield;

        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {

        sprint();

        movement();

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
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * speed * Time.deltaTime);

        HandleFootsteps();

        jump();

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

        if (shield <= 0)
        {
            shield = 0;

        }

        if (shield > 0)
        {

            shield -= amount;

            updatePlayerUI();

            StartCoroutine(ShieldDamageFlashScreen());

        }

        if (shield == 0 && armor > 0)
        {

            armor -= amount;

            updatePlayerUI();

            StartCoroutine(ArmorDamageFlashScreen());

        }
        if (shield == 0 && armor == 0)
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
        armor += amount;

        if (doesIncreaseMax)
        {
            maxArmor += amount;

        }
        else if (armor >= maxArmor && !doesIncreaseMax)
        {

            armor = maxArmor;
        }

        updatePlayerUI();

    }

    public void GainShield(int amount, bool doesIncreaseMax)
    {

        shield += amount;

        if (doesIncreaseMax)
        {
            maxShield += amount;

        }
        else if (shield >= maxShield && !doesIncreaseMax)
        {

            shield = maxShield;
        }

        updatePlayerUI();

    }

    public void GainAmmo(int amount, bool doesIncreaseMax)
    {
        reserveAmmo += amount;



        if (doesIncreaseMax)
        {
            reserveAmmo += amount;

        }
        updatePlayerUI();

    }

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

    public void updatePlayerUI()
    {    

        gamemanager.instance.playerHPBar.fillAmount = (float)HP / maxHP;
        gamemanager.instance.playerShieldBar.fillAmount = (float)shield / maxShield;
        gamemanager.instance.playerArmorBar.fillAmount = (float)armor / maxArmor;
        gamemanager.instance.ammoText.text = $"{equippedWeapon.GetAmmoInMag()} / {inventory.GetAmmoAmount("Ammo")}";
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

}