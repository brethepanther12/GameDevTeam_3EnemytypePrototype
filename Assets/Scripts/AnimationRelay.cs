using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public GruntAi gruntAi;
    public Enemy enemy;

    public void FootStep()
    {
        if (gruntAi != null)
            gruntAi.FootStep();
        else if (enemy != null)
            enemy.FootStep();
    }
}
