using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image bossHealthBarFill;

    void Update()
    {
        if (gamemanager.instance.currentBoss != null)
        {
            bossHealthBarFill.fillAmount = (float)gamemanager.instance.currentBoss.CurrentHealthPoints / gamemanager.instance.currentBoss.MaxHealthPoints;
        }
    }
}