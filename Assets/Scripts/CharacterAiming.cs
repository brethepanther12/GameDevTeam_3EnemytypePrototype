using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    [Header("Core References")]
    public Transform cameraTransform; 
    public Transform spineBone;      

    [Header("Tuning")]
    public Vector3 rotationOffset;

    private Transform leftHandTarget;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Weapon currentWeapon = transform.root.GetComponentInChildren<Weapon>();
        if (currentWeapon != null)
        {
            leftHandTarget = currentWeapon.leftHandGrip;
        }
        else
        {
            leftHandTarget = null;
        }
    }

    void LateUpdate()
    {
        if (spineBone != null && cameraTransform != null)
        {
            spineBone.rotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
}