using UnityEngine;
using System.Collections;


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
    [SerializeField] int ammo;
    [SerializeField] int maxAmmo;
    [SerializeField] int shield;
    [SerializeField] int maxShield;
    [SerializeField] int armor;
    [SerializeField] int maxArmor;
    [SerializeField] int shootDamage;
    [SerializeField] int meleeDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    private enum powerUpType
    {
        health, shield, armor, ammo, speed, jump, damage
    }

    bool hasKey;
    bool isPoweredUp;

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
        HPOrig = HP;
        armorOrig = armor;
        shieldOrig = shield;

        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        sprint();

        movement();
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

        jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (Input.GetButton("Fire1") && shootTimer > shootRate)
        {
            shoot();
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

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            //Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }

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

        if (HP >= maxHP && doesIncreaseMax)
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

        if (armor >= maxArmor && doesIncreaseMax)
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

        if (shield >= maxShield && doesIncreaseMax)
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
        ammo += amount;

        if (ammo >= maxAmmo && doesIncreaseMax)
        {
            maxAmmo += amount;

        }
        else if (ammo >= maxAmmo && !doesIncreaseMax)
        {

            ammo = maxAmmo;
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
        numKeys++;

        updatePlayerUI();
    }

    IEnumerator PowerUp(float duration)
    {
        isPoweredUp = true;
        yield return new WaitForSeconds(duration);
        isPoweredUp = false;
    }

    public void updatePlayerUI()
    {

        gamemanager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        gamemanager.instance.playerShieldBar.fillAmount = (float)shield / shieldOrig;
        gamemanager.instance.playerArmorBar.fillAmount = (float)armor / armorOrig;
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