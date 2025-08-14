using UnityEngine;
using System.Collections;

public class Destructible : MonoBehaviour, IDamage
{

    [SerializeField] int HP;
    [SerializeField] bool isDestructible;
    [SerializeField] Renderer model;

    Color colorOrig;

    public void takeDamage(int amount)
    {
        if (isDestructible)
        {
            HP -= amount;

            StartCoroutine(FlashWhite());

            if (HP <= 0 )
            {
                Destroy(gameObject);
            }
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

                takeDamage(amount);
                break;

            case DamageStatus.Corrosive:

                takeDamage(amount + 1);
                break;

            case DamageStatus.Cryo:

                takeDamage(amount);
                break;

            case DamageStatus.Electric:

                takeDamage(amount);
                break;

            case DamageStatus.Explosive:

                takeDamage(amount * 2);
                break;

            default:
                break;
        }



    }

    public IEnumerator FlashWhite()
    {
        model.material.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        model.material.color = colorOrig;
    }

    public void slowDown(float magnitude, float duration)
    {
        return;
    }
}
