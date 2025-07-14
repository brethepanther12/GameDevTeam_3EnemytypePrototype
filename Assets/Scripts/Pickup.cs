using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    private enum pickupType
    {
        ammo, health, armor, shield, damage, speed, jump, key
    };


    [SerializeField] int magnitude;

    //Quantity is for amount added, magnitude is for multiplying the amount(IE: Damage increase X2 or X3)

    [SerializeField] int quantity;
    [SerializeField] int duration;
    [SerializeField] bool increaseMax;

    //make sure to choose the right pickup type in the editor

    [SerializeField] pickupType pickup;

    bool canUse;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandlePickup(pickupType type)
    {

        playerController pc = gamemanager.instance.playerScript;

        if (pc == null)
        {
            return;

        }
        else
        {
            switch (type)
            {
                case pickupType.health:

                    pc.Heal(quantity, increaseMax);

                    break;

                case pickupType.ammo:

                    pc.GainAmmo(quantity, increaseMax);

                    break;

                case pickupType.shield:

                    pc.GainShield(quantity, increaseMax);

                    break;

                case pickupType.armor:

                    pc.GainArmor(quantity, increaseMax);

                    break;

                case pickupType.damage:

                    pc.IncreaseDamage(quantity, magnitude);

                    break;

                case pickupType.speed:

                    pc.IncreaseSpeed(quantity, magnitude);

                    break;

                case pickupType.jump:

                    pc.IncreaseJumpMaxCount(quantity, magnitude);

                    break;

                case pickupType.key:

                    pc.AddKey(quantity);

                    break;

                default:

                    break;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            HandlePickup(pickup);

            Destroy(gameObject);
        }
        
    }
}
