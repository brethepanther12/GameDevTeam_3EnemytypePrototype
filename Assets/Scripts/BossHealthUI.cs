using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image bossHealthBarFill;

    void Update()
    {
        // Check if gamemanager and currentBoss are valid
        if (gamemanager.instance == null || gamemanager.instance.currentBoss == null)
            return;

        // Avoid division by zero
        if (gamemanager.instance.currentBoss.MaxHealthPoints <= 0)
            return;

        // Update health bar fill amount
        bossHealthBarFill.fillAmount =
            (float)gamemanager.instance.currentBoss.CurrentHealthPoints / gamemanager.instance.currentBoss.MaxHealthPoints;
    }
}