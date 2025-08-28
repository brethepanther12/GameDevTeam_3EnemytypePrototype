using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    [SerializeField] int magnitude;

    //Quantity is for amount added, magnitude is for multiplying the amount(IE: Damage increase X2 or X3)

    [SerializeField] int quantity;
    [SerializeField] int duration;
    [SerializeField] bool increaseMax;

    //make sure to choose the right pickup type in the editor

    [SerializeField] UpgradeType pickup;
    [SerializeField] AudioClip pickupSound;

    bool canUse;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandlePickup(UpgradeType type)
    {

        playerController pc = gamemanager.instance.playerScript;
        PlayerInventory inv = pc.inventory;


        if (pc == null)
        {
            return;

        }
        else
        {
            switch (type)
            {
                case UpgradeType.Health:
                    if (pc.HP < pc.maxHP || increaseMax)
                        canUse = pc.Heal(quantity, increaseMax);
                    break;

                case UpgradeType.Shield:
                    if (pc.shield < pc.maxShield || increaseMax)
                        canUse = pc.GainShield(quantity, increaseMax);
                    break;

                case UpgradeType.Armor:
                    if (pc.armor < pc.maxArmor || increaseMax)
                        canUse = pc.GainArmor(quantity, increaseMax);
                    break;

                case UpgradeType.Damage:
                    pc.IncreaseDamage(quantity, magnitude);
                    canUse = true;
                    break;

                case UpgradeType.Speed:
                    pc.IncreaseSpeed(quantity, magnitude);
                    canUse = true;
                    break;

                case UpgradeType.Jump:
                    pc.IncreaseJumpMaxCount(quantity, magnitude);
                    canUse = true;
                    break;
            }
           
        }
        if (canUse)
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            HandlePickup(pickup);
           
            //Destroy(gameObject);
        }
        
    }
}
