using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image bossHealthBarFill;
    public BossAI boss;

    void Update()
    {
        if (boss != null)
        {
            bossHealthBarFill.fillAmount = (float)boss.CurrentHealthPoints / boss.MaxHealthPoints;
        }
    }
}