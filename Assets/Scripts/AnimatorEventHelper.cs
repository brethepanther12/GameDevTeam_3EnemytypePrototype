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
}