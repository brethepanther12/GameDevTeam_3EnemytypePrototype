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

    public IEnumerator FlashWhite()
    {
        model.material.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        model.material.color = colorOrig;
    }

}
