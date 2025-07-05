using UnityEngine;

public class AnimatorEventHelper : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void SetBoolTrue(string paramName)
    {
        animator.SetBool(paramName, true);
    }

    public void SetBoolFalse(string paramName)
    {
        animator.SetBool(paramName, false);
    }

    void OnEnable()
    {
        if (animator != null)
        {
            animator.Update(0f);
            animator.Play("Idle", 0, 0f);
        }
    }
}